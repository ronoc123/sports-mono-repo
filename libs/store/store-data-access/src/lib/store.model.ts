export interface ProductDto {
  id: string;
  name: string;
  description: string;
  quantity: number;
  priceAmount: number;
  priceCurrency: string;
}

export interface CreatePurchaseRequest {
  userId: string;
  organizationId: string;
  productId: string;
}

export interface CreatePurchaseResponse {
  purchaseId: string;
  clientSecret: string;
}
