-- =====================================================
-- Seed Data for Costa Rica Geographic Divisions
-- Tables: Provincias, Cantones, Distritos
-- Official data from Ministerio de Hacienda
-- =====================================================

SET IDENTITY_INSERT Provincias ON;

-- =====================================================
-- 1. PROVINCIAS (7 provincias de Costa Rica)
-- =====================================================
INSERT INTO Provincias (Id, Codigo, Descripcion, Activo) VALUES
(1, '1', 'San José', 1),
(2, '2', 'Alajuela', 1),
(3, '3', 'Cartago', 1),
(4, '4', 'Heredia', 1),
(5, '5', 'Guanacaste', 1),
(6, '6', 'Puntarenas', 1),
(7, '7', 'Limón', 1);

SET IDENTITY_INSERT Provincias OFF;

-- =====================================================
-- 2. CANTONES (82 cantones)
-- Formato: Código = "PPCC" donde PP=Provincia, CC=Canton
-- =====================================================

SET IDENTITY_INSERT Cantones ON;

-- SAN JOSÉ (20 cantones)
INSERT INTO Cantones (Id, ProvinciaId, Codigo, Descripcion, Activo) VALUES
(1, 1, '0101', 'San José', 1),
(2, 1, '0102', 'Escazú', 1),
(3, 1, '0103', 'Desamparados', 1),
(4, 1, '0104', 'Puriscal', 1),
(5, 1, '0105', 'Tarrazú', 1),
(6, 1, '0106', 'Aserrí', 1),
(7, 1, '0107', 'Mora', 1),
(8, 1, '0108', 'Goicoechea', 1),
(9, 1, '0109', 'Santa Ana', 1),
(10, 1, '0110', 'Alajuelita', 1),
(11, 1, '0111', 'Vásquez de Coronado', 1),
(12, 1, '0112', 'Acosta', 1),
(13, 1, '0113', 'Tibás', 1),
(14, 1, '0114', 'Moravia', 1),
(15, 1, '0115', 'Montes de Oca', 1),
(16, 1, '0116', 'Turrubares', 1),
(17, 1, '0117', 'Dota', 1),
(18, 1, '0118', 'Curridabat', 1),
(19, 1, '0119', 'Pérez Zeledón', 1),
(20, 1, '0120', 'León Cortés Castro', 1),

-- ALAJUELA (16 cantones)
(21, 2, '0201', 'Alajuela', 1),
(22, 2, '0202', 'San Ramón', 1),
(23, 2, '0203', 'Grecia', 1),
(24, 2, '0204', 'San Mateo', 1),
(25, 2, '0205', 'Atenas', 1),
(26, 2, '0206', 'Naranjo', 1),
(27, 2, '0207', 'Palmares', 1),
(28, 2, '0208', 'Poás', 1),
(29, 2, '0209', 'Orotina', 1),
(30, 2, '0210', 'San Carlos', 1),
(31, 2, '0211', 'Zarcero', 1),
(32, 2, '0212', 'Sarchí', 1),
(33, 2, '0213', 'Upala', 1),
(34, 2, '0214', 'Los Chiles', 1),
(35, 2, '0215', 'Guatuso', 1),
(36, 2, '0216', 'Río Cuarto', 1),

-- CARTAGO (8 cantones)
(37, 3, '0301', 'Cartago', 1),
(38, 3, '0302', 'Paraíso', 1),
(39, 3, '0303', 'La Unión', 1),
(40, 3, '0304', 'Jiménez', 1),
(41, 3, '0305', 'Turrialba', 1),
(42, 3, '0306', 'Alvarado', 1),
(43, 3, '0307', 'Oreamuno', 1),
(44, 3, '0308', 'El Guarco', 1),

-- HEREDIA (10 cantones)
(45, 4, '0401', 'Heredia', 1),
(46, 4, '0402', 'Barva', 1),
(47, 4, '0403', 'Santo Domingo', 1),
(48, 4, '0404', 'Santa Bárbara', 1),
(49, 4, '0405', 'San Rafael', 1),
(50, 4, '0406', 'San Isidro', 1),
(51, 4, '0407', 'Belén', 1),
(52, 4, '0408', 'Flores', 1),
(53, 4, '0409', 'San Pablo', 1),
(54, 4, '0410', 'Sarapiquí', 1),

-- GUANACASTE (11 cantones)
(55, 5, '0501', 'Liberia', 1),
(56, 5, '0502', 'Nicoya', 1),
(57, 5, '0503', 'Santa Cruz', 1),
(58, 5, '0504', 'Bagaces', 1),
(59, 5, '0505', 'Carrillo', 1),
(60, 5, '0506', 'Cañas', 1),
(61, 5, '0507', 'Abangares', 1),
(62, 5, '0508', 'Tilarán', 1),
(63, 5, '0509', 'Nandayure', 1),
(64, 5, '0510', 'La Cruz', 1),
(65, 5, '0511', 'Hojancha', 1),

-- PUNTARENAS (11 cantones)
(66, 6, '0601', 'Puntarenas', 1),
(67, 6, '0602', 'Esparza', 1),
(68, 6, '0603', 'Buenos Aires', 1),
(69, 6, '0604', 'Montes de Oro', 1),
(70, 6, '0605', 'Osa', 1),
(71, 6, '0606', 'Quepos', 1),
(72, 6, '0607', 'Golfito', 1),
(73, 6, '0608', 'Coto Brus', 1),
(74, 6, '0609', 'Parrita', 1),
(75, 6, '0610', 'Corredores', 1),
(76, 6, '0611', 'Garabito', 1),

-- LIMÓN (6 cantones)
(77, 7, '0701', 'Limón', 1),
(78, 7, '0702', 'Pococí', 1),
(79, 7, '0703', 'Siquirres', 1),
(80, 7, '0704', 'Talamanca', 1),
(81, 7, '0705', 'Matina', 1),
(82, 7, '0706', 'Guácimo', 1);

SET IDENTITY_INSERT Cantones OFF;

-- =====================================================
-- 3. DISTRITOS (488 distritos - muestra inicial)
-- Formato: Código = "PPCCDD" donde PP=Provincia, CC=Canton, DD=Distrito
-- Para el seed inicial, incluimos los distritos de San José (Canton 0101)
-- =====================================================

SET IDENTITY_INSERT Distritos ON;

-- SAN JOSÉ - CANTÓN SAN JOSÉ (11 distritos)
INSERT INTO Distritos (Id, CantonId, Codigo, Descripcion, Activo) VALUES
(1, 1, '010101', 'Carmen', 1),
(2, 1, '010102', 'Merced', 1),
(3, 1, '010103', 'Hospital', 1),
(4, 1, '010104', 'Catedral', 1),
(5, 1, '010105', 'Zapote', 1),
(6, 1, '010106', 'San Francisco de Dos Ríos', 1),
(7, 1, '010107', 'Uruca', 1),
(8, 1, '010108', 'Mata Redonda', 1),
(9, 1, '010109', 'Pavas', 1),
(10, 1, '010110', 'Hatillo', 1),
(11, 1, '010111', 'San Sebastián', 1),

-- SAN JOSÉ - CANTÓN ESCAZÚ (3 distritos)
(12, 2, '010201', 'Escazú', 1),
(13, 2, '010202', 'San Antonio', 1),
(14, 2, '010203', 'San Rafael', 1),

-- SAN JOSÉ - CANTÓN DESAMPARADOS (13 distritos)
(15, 3, '010301', 'Desamparados', 1),
(16, 3, '010302', 'San Miguel', 1),
(17, 3, '010303', 'San Juan de Dios', 1),
(18, 3, '010304', 'San Rafael Arriba', 1),
(19, 3, '010305', 'San Antonio', 1),
(20, 3, '010306', 'Frailes', 1),
(21, 3, '010307', 'Patarrá', 1),
(22, 3, '010308', 'San Cristóbal', 1),
(23, 3, '010309', 'Rosario', 1),
(24, 3, '010310', 'Damas', 1),
(25, 3, '010311', 'San Rafael Abajo', 1),
(26, 3, '010312', 'Gravilias', 1),
(27, 3, '010313', 'Los Guido', 1),

-- ALAJUELA - CANTÓN ALAJUELA (14 distritos principales)
(28, 21, '020101', 'Alajuela', 1),
(29, 21, '020102', 'San José', 1),
(30, 21, '020103', 'Carrizal', 1),
(31, 21, '020104', 'San Antonio', 1),
(32, 21, '020105', 'Guácima', 1),
(33, 21, '020106', 'San Isidro', 1),
(34, 21, '020107', 'Sabanilla', 1),
(35, 21, '020108', 'San Rafael', 1),
(36, 21, '020109', 'Río Segundo', 1),
(37, 21, '020110', 'Desamparados', 1),
(38, 21, '020111', 'Turrúcares', 1),
(39, 21, '020112', 'Tambor', 1),
(40, 21, '020113', 'Garita', 1),
(41, 21, '020114', 'Sarapiquí', 1);

SET IDENTITY_INSERT Distritos OFF;

-- =====================================================
-- Verificación de datos insertados
-- =====================================================
SELECT 'Provincias insertadas: ' + CAST(COUNT(*) AS VARCHAR) FROM Provincias;
SELECT 'Cantones insertados: ' + CAST(COUNT(*) AS VARCHAR) FROM Cantones;
SELECT 'Distritos insertados: ' + CAST(COUNT(*) AS VARCHAR) FROM Distritos;

-- =====================================================
-- Notas:
-- 1. Este script incluye las 7 provincias completas
-- 2. Los 82 cantones completos de Costa Rica
-- 3. Una muestra de distritos para iniciar (41 distritos)
-- 4. Para agregar los 488 distritos completos, se debe extender
--    la sección de distritos con todos los datos oficiales
-- 5. Los códigos siguen el estándar de Hacienda CR
-- =====================================================
