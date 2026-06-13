export interface UserDto {
  customerId: number;
  clientId: number | null;
  fullName: string;
  phone: string;
}

export interface AuthResponse {
  token: string;
  user: UserDto;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
}

export interface CarDto {
  carId: number;
  brand: string;
  model: string;
  manufactureYear: number;
  licensePlate: string;
  vin: string;
  photoUrl: string | null;
  additionalPhotosJson: string | null;
  fullInfo: string;
}

export interface ServiceDto {
  serviceCode: number;
  serviceName: string;
  serviceCost: number;
  fixedDurationMin: number | null;
}

export interface TimeSlotDto {
  time: string;
  available: boolean;
}

export interface CompletedWorkDto {
  workId: number;
  serviceCode: number;
  serviceName: string;
  workTotal: number;
  wheelCount: number;
}

export interface OrderDto {
  orderNumber: number;
  orderDate: string;
  scheduledAt: string | null;
  status: string;
  paymentStatus: string;
  clientTotal: number | null;
  car: CarDto | null;
  masterName: string | null;
  services: CompletedWorkDto[];
  tireInfo?: string | null;
}

export interface TireDto {
  tireId: number;
  tireType: string;
  seasonality: string;
  manufacturer: string;
  tireModel: string;
  size: string;
  loadIndex: number;
  wearPercentage: number;
  pressure: number;
  fullInfo: string;
}

export interface CreateOrderRequest {
  carId?: number | null;
  tireId?: number | null;
  serviceCodes: number[] | null;
  hasOther: boolean;
  description: string | null;
  scheduledAt: string | null;
  wheelCount?: number;
}

export interface EditOrderRequest {
  serviceCodes: number[] | null;
  hasOther: boolean;
  description: string | null;
  scheduledAt: string | null;
  wheelCount?: number;
}

export interface OcrResult {
  brand: string;
  model: string;
  year: string;
  vin: string;
  licensePlate: string;
}

export interface PublicStatsDto {
  carsServed: number;
  ordersTotal: number;
  clientsTotal: number;
  satisfactionPercent: number | null;
  branchesCount: number;
  isOpen24h: boolean;
}

export interface ReviewDto {
  reviewId: number;
  author: string;
  rating: number;
  text: string;
  carModel: string | null;
  orderNumber: number | null;
  createdAt: string;
}

export interface CreateReviewRequest {
  rating: number;
  text: string;
  carModel?: string | null;
  orderNumber?: number | null;
}
