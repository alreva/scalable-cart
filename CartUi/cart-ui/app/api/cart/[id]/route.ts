import { NextResponse } from "next/server";
import { CartResponseDto } from "../../../components/cartDto";


export async function GET(
  req: Request,
  { params }: { params: { id: string } }
) {
  const { id } = params;
  const res = await fetch('http://localhost:5254/cart/' + id, {
    cache: 'no-store',
  });

  console.log(res);
  console.log(res.headers);
  const data = await res.json() as CartResponseDto;
  console.log(data);
  return NextResponse.json(data);
}
