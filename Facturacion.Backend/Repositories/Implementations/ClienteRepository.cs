using Facturacion.Backend.Data;
using Facturacion.Backend.Repositories.Interfaces;
using Facturacion.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Backend.Repositories.Implementations;

public class ClienteRepository : IClienteRepository
{
    private readonly DataContext _context;

    public ClienteRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> GetAsync(Guid id)
    {
        return await _context.Clientes
            .Include(c => c.Empresa)
            .Include(c => c.Telefonos)
            .Include(c => c.Emails)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<IEnumerable<Cliente>> GetByEmpresaAsync(Guid empresaId)
    {
        return await _context.Clientes
            .Include(c => c.Empresa)
            .Include(c => c.Emails)
            .Where(c => c.EmpresaId == empresaId && !c.IsDeleted)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Cliente?> GetByIdentificationAsync(Guid empresaId, string numeroIdentificacion)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && 
                                     c.NumeroIdentificacion == numeroIdentificacion && 
                                     !c.IsDeleted);
    }

    public async Task<Cliente> AddAsync(Cliente cliente)
    {
        cliente.FechaCreacion = DateTime.UtcNow;
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task UpdateAsync(Cliente cliente)
    {
        cliente.FechaModificacion = DateTime.UtcNow;

        // Obtener el cliente existente con sus emails
        var clienteExistente = await _context.Clientes
            .Include(c => c.Emails)
            .FirstOrDefaultAsync(c => c.Id == cliente.Id);

        if (clienteExistente != null)
        {
            // Actualizar propiedades del cliente
            _context.Entry(clienteExistente).CurrentValues.SetValues(cliente);

            // Manejar emails: eliminar los que ya no están y agregar/actualizar los nuevos
            if (cliente.Emails != null)
            {
                // Eliminar emails que ya no están en la lista
                var emailsAEliminar = clienteExistente.Emails!
                    .Where(e => !cliente.Emails.Any(ne => ne.Id == e.Id))
                    .ToList();

                foreach (var email in emailsAEliminar)
                {
                    _context.Emails.Remove(email);
                }

                // Agregar o actualizar emails
                foreach (var email in cliente.Emails)
                {
                    email.ClienteId = cliente.Id;

                    if (email.Id == 0)
                    {
                        // Nuevo email
                        clienteExistente.Emails!.Add(email);
                    }
                    else
                    {
                        // Actualizar email existente
                        var emailExistente = clienteExistente.Emails!.FirstOrDefault(e => e.Id == email.Id);
                        if (emailExistente != null)
                        {
                            _context.Entry(emailExistente).CurrentValues.SetValues(email);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
        else
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid id, string userId)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            cliente.IsDeleted = true;
            cliente.FechaEliminacion = DateTime.UtcNow;
            cliente.UsuarioEliminacionId = userId;
            await _context.SaveChangesAsync();
        }
    }
}
