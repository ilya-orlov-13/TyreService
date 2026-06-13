export interface LocalService {
  id: string;
  name: string;
  category: string;
  priceR13_14: number;
  priceR15_16: number;
  priceR17_18: number;
  priceR19_20: number;
  priceR21_plus: number;
  description: string;
}

export const INITIAL_SERVICES: LocalService[] = [
  {
    id: '1',
    name: 'Комплексный шиномонтаж (4 колеса)',
    category: 'tire-fitting',
    priceR13_14: 1800, priceR15_16: 2200, priceR17_18: 2800, priceR19_20: 3600, priceR21_plus: 4500,
    description: 'Полный перечень работ: снятие/установка, демонтаж/монтаж шины, замена вентиля, балансировка и грузики.'
  },
  {
    id: '2',
    name: 'Балансировка колес (сезонный уход / проверка)',
    category: 'balancing',
    priceR13_14: 800, priceR15_16: 1000, priceR17_18: 1200, priceR19_20: 1600, priceR21_plus: 2000,
    description: 'Высокоточное выравнивание массы колеса на компьютерном 3D-стенде с учетом дорожных нагрузок.'
  },
  {
    id: '3',
    name: 'Правка литых дисков',
    category: 'repair',
    priceR13_14: 1200, priceR15_16: 1500, priceR17_18: 1800, priceR19_20: 2500, priceR21_plus: 3500,
    description: 'Восстановление геометрии диска на гидравлическом станке.'
  },
  {
    id: '4',
    name: 'Вулканизация и ремонт сложных порезов',
    category: 'repair',
    priceR13_14: 900, priceR15_16: 1100, priceR17_18: 1300, priceR19_20: 1700, priceR21_plus: 2200,
    description: 'Горячая вулканизация боковых порезов и грыж беговой дорожки.'
  },
  {
    id: '5',
    name: 'Сезонное хранение комплекта колес (6 месяцев)',
    category: 'storage',
    priceR13_14: 2500, priceR15_16: 3000, priceR17_18: 3500, priceR19_20: 4200, priceR21_plus: 5000,
    description: 'Хранение в крытом сухом боксе с температурой +15°C.'
  },
  {
    id: '6',
    name: 'Чернение шин и полировка бортов',
    category: 'other',
    priceR13_14: 400, priceR15_16: 400, priceR17_18: 500, priceR19_20: 600, priceR21_plus: 800,
    description: 'Гидрофобное нано-покрытие для защиты от ультрафиолета.'
  }
];
