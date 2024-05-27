import { NextRequest, NextResponse } from "next/server";
import internal from "stream";

export async function GET(
  req: NextRequest,
  { params }: { params: { id: string } }
) {
  console.log("params:", params);
  const { id } = params;
  const { searchParams } = new URL(req.url)
  const skip = parseInt(searchParams.get("skip") || "0");
  const take = parseInt(searchParams.get("take") || "8");
  let baseUrl = new URL(
    `http://localhost:5254/catalog/category/${id}/products`
  );
  if (skip) {
    baseUrl.searchParams.append("skip", skip.toString());
  }
  if (take) {
    baseUrl.searchParams.append("take", take.toString());
  }
  console.log("baseUrl:", baseUrl.toString());
  const response = await fetch(baseUrl.toString());
  const data = await response.json();
  console.log("data:", data);
  return NextResponse.json(data);
}
