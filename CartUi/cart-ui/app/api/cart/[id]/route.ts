import { NextResponse } from "next/server";
import { CartResponseDto } from "../../../components/cartDto";
import { API_URL } from "@/app/env";


export async function GET(
  req: Request,
  { params }: { params: { id: string } }
) {
  const { id } = params;
  const res = await fetch(`${API_URL}/cart/` + id, {
    cache: 'no-store',
  });

  console.log(res);
  console.log(res.headers);
  const data = await res.json() as CartResponseDto;
  console.log(data);
  return NextResponse.json(data);
}
