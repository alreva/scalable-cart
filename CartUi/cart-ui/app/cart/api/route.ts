import { NextResponse } from "next/server";
import { CartResponse } from "../cartDto";


export async function GET() {
  const res = await fetch('http://localhost:5254/cart/1');
  const data = await res.json() as CartResponse;
  
  console.log(data);

  return NextResponse.json(data.details);
}