-- Script para insertar catálogo de Impuestos de Hacienda Costa Rica
-- Ejecutar solo si la tabla Impuestos está vacía

-- Verificar si existen registros
IF NOT EXISTS (SELECT 1 FROM Impuestos)
BEGIN
    PRINT 'Insertando catálogo de Impuestos...'

    INSERT INTO Impuestos (Codigo, Descripcion, Porcentaje, Activo)
    VALUES
        ('01', 'Impuesto al Valor Agregado', 13.00, 1),
        ('02', 'Impuesto Selectivo de Consumo', 0.00, 1),
        ('03', 'Impuesto Único a los Combustibles', 0.00, 1),
        ('04', 'Impuesto específico de Bebidas Alcohólicas', 0.00, 1),
        ('05', 'Impuesto Específico sobre las bebidas envasadas sin contenido alcohólico y jabones de tocador', 0.00, 1),
        ('06', 'Impuesto a los Productos de Tabaco', 0.00, 1),
        ('07', 'IVA (cálculo especial)', 13.00, 1),
        ('08', 'IVA Régimen de Bienes Usados (Factor)', 13.00, 1),
        ('12', 'Impuesto específico al cemento asfáltico', 0.00, 1),
        ('99', 'Otros', 0.00, 1);

    PRINT 'Catálogo de Impuestos insertado correctamente.'
END
ELSE
BEGIN
    PRINT 'La tabla Impuestos ya contiene datos. No se insertarán registros.'
END
GO

-- Verificar registros insertados
SELECT * FROM Impuestos ORDER BY Codigo;
GO
