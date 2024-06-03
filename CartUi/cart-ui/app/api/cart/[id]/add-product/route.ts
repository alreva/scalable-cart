import { API_URL } from "@/app/env";
import { NextResponse } from "next/server";

export async function POST(
  req: Request,
  { params }: { params: { id: number } }
) {
  const { id } = params;
  const productModel = await req.json();
  const res = await fetch(
    `${API_URL}/cart/${id}/add-product`, {
    headers: {
      'Content-Type': 'application/json',
    },
    method: 'POST',
    body: JSON.stringify(productModel),
  });
  console.log("Product:", productModel);
  console.log("Response:", res );
  return NextResponse.json({});
}
