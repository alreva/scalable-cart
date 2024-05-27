import { NextResponse } from "next/server";

export async function POST(
  req: Request,
  { params }: { params: { id: number } }
) {
  const { id } = params;
  const productModel = await req.json();
  const res = await fetch(
    `http://localhost:5254/cart/${id}/add-product`, {
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
