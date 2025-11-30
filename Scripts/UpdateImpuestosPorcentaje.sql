-- Script para actualizar los porcentajes de impuestos en la tabla Impuestos
-- Ejecutar este script si los porcentajes están en 0

UPDATE Impuestos SET Porcentaje = 13.00 WHERE Codigo = '01'; -- IVA
UPDATE Impuestos SET Porcentaje = 0.00 WHERE Codigo = '02';  -- Selectivo de Consumo (variable)
UPDATE Impuestos SET Porcentaje = 0.00 WHERE Codigo = '03';  -- Único a Combustibles (variable)
UPDATE Impuestos SET Porcentaje = 0.00 WHERE Codigo = '04';  -- Bebidas Alcohólicas (variable)
UPDATE Impuestos SET Porcentaje = 0.00 WHERE Codigo = '05';  -- Bebidas sin alcohol (variable)
UPDATE Impuestos SET Porcentaje = 0.00 WHERE Codigo = '06';  -- Tabaco (variable)
UPDATE Impuestos SET Porcentaje = 13.00 WHERE Codigo = '07'; -- IVA cálculo especial
UPDATE Impuestos SET Porcentaje = 13.00 WHERE Codigo = '08'; -- IVA Bienes Usados
UPDATE Impuestos SET Porcentaje = 0.00 WHERE Codigo = '12';  -- Cemento asfáltico (variable)
UPDATE Impuestos SET Porcentaje = 0.00 WHERE Codigo = '99';  -- Otros

-- Verificar los cambios
SELECT Codigo, Descripcion, Porcentaje, Activo FROM Impuestos ORDER BY Codigo;
