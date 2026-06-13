export function cleanPhone(value: string): string {
  return value.replace(/\D/g, '');
}

export function formatPhone(value: string): string {
  let digits = value.replace(/\D/g, '');
  if (digits.length === 0) return '';
  if (digits[0] !== '7') digits = '7' + digits;
  digits = digits.substring(0, 11);
  let formatted = '+7';
  if (digits.length > 1) {
    formatted += ' (' + digits.substring(1, Math.min(4, digits.length));
  }
  if (digits.length >= 4) {
    formatted += ') ' + digits.substring(4, Math.min(7, digits.length));
  }
  if (digits.length >= 7) {
    formatted += '-' + digits.substring(7, Math.min(9, digits.length));
  }
  if (digits.length >= 9) {
    formatted += '-' + digits.substring(9, 11);
  }
  return formatted;
}

const RUSSIAN_PLATE_LETTERS = 'АВЕКМНОРСТУХ';
const ENG_TO_RUS: Record<string, string> = {
  'A': 'А', 'B': 'В', 'E': 'Е', 'K': 'К', 'M': 'М',
  'H': 'Н', 'O': 'О', 'P': 'Р', 'C': 'С', 'T': 'Т',
  'Y': 'У', 'X': 'Х'
};

export function formatLicensePlate(value: string): string {
  let s = value.toUpperCase();
  s = s.split('').map(ch => ENG_TO_RUS[ch] || ch).join('');

  const letters = new Set(RUSSIAN_PLATE_LETTERS);
  const digits = new Set('0123456789');

  let result = '';
  for (const ch of s) {
    if (result.length === 0 && letters.has(ch)) {
      result += ch;
    } else if (result.length >= 1 && result.length <= 3 && digits.has(ch)) {
      result += ch;
    } else if ((result.length === 4 || result.length === 5) && letters.has(ch)) {
      result += ch;
    } else if (result.length >= 6 && result.length < 9 && digits.has(ch)) {
      result += ch;
    }
    if (result.length >= 9) break;
  }
  return result;
}
