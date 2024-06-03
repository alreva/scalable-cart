import { API_URL } from "@/app/env";
import { NextRequest, NextResponse } from "next/server";

export async function GET(
  req: NextRequest,
  res: NextResponse
) {
  try {
    const response = await fetch(
      `${API_URL}/catalog/top-categories`
    );
    const data = await response.json();
    return NextResponse.json(data);
  } catch (error) {
    console.error("Error executing M2M request:", error);
    return NextResponse.error();
  }
}
