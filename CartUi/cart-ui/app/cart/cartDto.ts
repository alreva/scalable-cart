export interface CartResponse {
    path: string;
    details: CartDetails;
}

export interface CartDetails {
    cartId: number;
    lineItems: LineItem[];
    totalPrice: number;
}

export interface LineItem {
    productName: string;
    price: number;
    quantity: number;
}
