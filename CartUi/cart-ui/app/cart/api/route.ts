import { NextResponse } from "next/server";
import { CartResponseDto } from "../../components/cartDto";


export async function GET(request: Request) {
  const { searchParams } = new URL(request.url)
  const id = searchParams.get('id');
  console.log('GET /cart/api/' + id);
  const res = await fetch('http://localhost:5254/cart/' + id, {
    cache: 'no-store',
  });

  console.log(res);
  console.log(res.headers);

  const data = await res.json() as CartResponseDto;
  
  console.log(data);

  return NextResponse.json(data);
}