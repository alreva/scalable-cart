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
    productName: string;
    price: number;
    quantity: number;
}
