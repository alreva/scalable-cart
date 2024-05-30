export interface CartResponseDto {
    path: string;
    details: CartDetailsDto;
}

export interface CartDetailsDto {
    cartId: number;
    lineItems: LineItemDto[];
    totalPrice: number;
}

export interface LineItemDto {
    productId: number;
    price: number;
    quantity: number;
}
