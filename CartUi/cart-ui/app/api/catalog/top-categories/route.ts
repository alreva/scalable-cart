import { NextRequest, NextResponse } from "next/server";

export async function GET(
  req: NextRequest,
  res: NextResponse
) {
  try {
    const response = await fetch(
      "http://localhost:5254/catalog/top-categories"
    );
    const data = await response.json();
    return NextResponse.json(data);
  } catch (error) {
    console.error("Error executing M2M request:", error);
    return NextResponse.error();
  }
}
