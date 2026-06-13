export const BRANDS = [
  'LADA (ВАЗ)', 'Toyota', 'Hyundai', 'Kia', 'Renault', 'Nissan', 'Volkswagen',
  'BMW', 'Mercedes-Benz', 'Audi', 'Ford', 'Chevrolet', 'Honda', 'Mazda',
  'Mitsubishi', 'Subaru', 'Suzuki', 'Lexus', 'Infiniti', 'Skoda', 'Volvo',
  'Opel', 'Peugeot', 'Citroen', 'Fiat', 'Jeep', 'Land Rover', 'Jaguar',
  'Mini', 'Porsche', 'Tesla', 'Cadillac', 'Dodge', 'Chrysler',
  'Chery', 'Geely', 'Haval', 'BYD', 'Changan', 'Great Wall', 'GAC',
  'UAZ', 'GAZ', 'Moskvich', 'Seat', 'Alfa Romeo', 'Ferrari', 'Lamborghini',
  'Bentley', 'Aston Martin', 'Maserati', 'Rolls-Royce', 'SsangYong',
  'Daihatsu', 'Isuzu', 'Iveco', 'MAN', 'Scania'
];

export const MODELS_BY_BRAND: Record<string, string[]> = {
  'LADA (ВАЗ)': ['Granta', 'Vesta', 'Niva', 'Niva Travel', 'Largus', 'XRAY', 'Kalina', 'Priora', '2107', '2114', '2110', '2109', '2121', '2131'],
  'Toyota': ['Camry', 'Corolla', 'RAV4', 'Land Cruiser', 'Land Cruiser Prado', 'Highlander', 'Fortuner', 'Hilux', 'Yaris', 'C-HR', 'Avensis', 'Verso', 'Alphard', 'Crown', 'Mark X', 'Supra'],
  'Hyundai': ['Solaris', 'Elantra', 'Tucson', 'Santa Fe', 'Creta', 'Sonata', 'ix35', 'Getz', 'Accent', 'Palisade', 'Kona', 'Staria', 'Porter'],
  'Kia': ['Rio', 'Sportage', 'Sorento', 'Optima', 'Ceed', 'Cerato', 'Soul', 'Mohave', 'Stinger', 'Picanto', 'Carnival', 'K5', 'Seltos'],
  'Renault': ['Logan', 'Sandero', 'Duster', 'Kaptur', 'Arkana', 'Megan', 'Fluence', 'Koleos', 'Laguna', 'Scenic', 'Master'],
  'Nissan': ['Qashqai', 'X-Trail', 'Murano', 'Juke', 'Pathfinder', 'Patrol', 'Navara', 'Teana', 'Almera', 'Note', 'Micra', 'Leaf', 'GT-R'],
  'Volkswagen': ['Polo', 'Passat', 'Tiguan', 'Touareg', 'Golf', 'Jetta', 'Teramont', 'Caddy', 'Transporter', 'Amarok', 'ID.4', 'Taos'],
  'BMW': ['X3', 'X5', 'X1', 'X6', '3 Series', '5 Series', '7 Series', 'X4', 'X7', 'M5', 'M3', '1 Series', 'iX', 'i4'],
  'Mercedes-Benz': ['E-Class', 'C-Class', 'S-Class', 'GLC', 'GLE', 'GLS', 'A-Class', 'GLK', 'ML', 'G-Class', 'Sprinter', 'Vito', 'EQS', 'EQE'],
  'Audi': ['Q5', 'Q7', 'A4', 'A6', 'Q3', 'A3', 'A8', 'Q8', 'TT', 'R8', 'e-tron', 'Q2', 'A5', 'A7'],
  'Ford': ['Focus', 'Kuga', 'Explorer', 'Escape', 'Mondeo', 'Fiesta', 'Fusion', 'Ranger', 'Transit', 'Mustang', 'Edge', 'Everest'],
  'Chevrolet': ['Cruze', 'Lacetti', 'Aveo', 'Niva', 'Captiva', 'Tahoe', 'Malibu', 'Spark', 'TrailBlazer', 'Camaro', 'Suburban', 'Traverse'],
  'Mazda': ['CX-5', 'CX-7', '3', '6', 'CX-9', 'CX-30', 'MX-5', 'RX-8', 'CX-3', '2', 'CX-60'],
  'Mitsubishi': ['Outlander', 'Pajero', 'L200', 'ASX', 'Lancer', 'Eclipse Cross', 'Montero', 'i-MiEV', 'Mirage', 'Delica'],
  'Subaru': ['Forester', 'Outback', 'Impreza', 'Legacy', 'XV', 'WRX', 'Tribeca', 'BRZ', 'Ascent'],
  'Skoda': ['Octavia', 'Rapid', 'Kodiaq', 'Fabia', 'Yeti', 'Superb', 'Karoq', 'Kamiq', 'Enyaq', 'Scala'],
  'Volvo': ['XC60', 'XC90', 'XC40', 'S60', 'S90', 'V60', 'V90', 'S40', 'C40', 'EX30', 'EX90'],
  'Lexus': ['RX', 'NX', 'LX', 'ES', 'IS', 'GX', 'UX', 'LS', 'RC', 'LC'],
  'Chery': ['Tiggo 7', 'Tiggo 8', 'Tiggo 4', 'Arrizo 8', 'Tiggo 5', 'Arrizo 5', 'Tiggo 9'],
  'Geely': ['Coolray', 'Atlas', 'Monjaro', 'Tugella', 'Emgrand', 'Preface', 'Okavango'],
  'Haval': ['Jolion', 'F7', 'H6', 'F7x', 'Dargo', 'H9', 'M6', 'H2'],
  'BYD': ['Tang', 'Han', 'Atto 3', 'Seal', 'Dolphin', 'Song Plus', 'Yuan Plus'],
  'Changan': ['CS35', 'CS55', 'CS75', 'UNI-K', 'UNI-T', 'Eado', 'Alsvin', 'Raeton']
};

export function getModelSuggestions(brand: string): string[] {
  if (brand && MODELS_BY_BRAND[brand]) {
    return MODELS_BY_BRAND[brand];
  }
  const all: string[] = [];
  for (const m of Object.values(MODELS_BY_BRAND)) {
    all.push(...m);
  }
  return [...new Set(all)].sort();
}

export function getAllPhotos(photoUrl: string | null, additionalPhotosJson: string | null): string[] {
  const urls: string[] = [];
  if (photoUrl) urls.push(photoUrl);
  if (additionalPhotosJson) {
    try {
      const parsed = JSON.parse(additionalPhotosJson);
      if (Array.isArray(parsed)) urls.push(...parsed);
    } catch { /* ignore */ }
  }
  return urls;
}

const CAR_ICON_BRAND_ALIASES: Record<string, string> = {
  'LADA (ВАЗ)': 'Lada',
};

export function getCarIconUrl(brand: string, model: string): string | null {
  const fsafe = (s: string) => s.replace(/\s+/g, '_');
  const iconBrand = CAR_ICON_BRAND_ALIASES[brand] ?? brand;
  return `/images/icons/cars/${fsafe(iconBrand)}_${fsafe(model)}.png`;
}
