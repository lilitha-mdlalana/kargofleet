import request from "supertest";
import { afterEach, describe, expect, it } from "vitest";
import { createApp } from "../app";
import { prisma } from "../db/client";

const app = createApp();
const TEST_PHONE = "+27000000001";

async function readOtpCode(phone: string): Promise<string> {
  const user = await prisma.user.findUniqueOrThrow({ where: { phone } });
  if (!user.otpCode) throw new Error("expected an OTP code to be set");
  return user.otpCode;
}

afterEach(async () => {
  await prisma.user.deleteMany({ where: { phone: TEST_PHONE } });
});

describe("POST /auth/signup", () => {
  it("creates a user and returns 202", async () => {
    const response = await request(app)
      .post("/auth/signup")
      .send({ name: "Test Driver", phone: TEST_PHONE });

    expect(response.status).toBe(202);
    const user = await prisma.user.findUnique({ where: { phone: TEST_PHONE } });
    expect(user).not.toBeNull();
  });

  it("rejects a duplicate phone number", async () => {
    await request(app).post("/auth/signup").send({ name: "Test Driver", phone: TEST_PHONE });

    const response = await request(app)
      .post("/auth/signup")
      .send({ name: "Test Driver", phone: TEST_PHONE });

    expect(response.status).toBe(400);
  });

  it("rejects a missing name", async () => {
    const response = await request(app).post("/auth/signup").send({ phone: TEST_PHONE });
    expect(response.status).toBe(400);
  });
});

describe("POST /auth/login", () => {
  it("rejects an unknown phone number", async () => {
    const response = await request(app).post("/auth/login").send({ phone: TEST_PHONE });
    expect(response.status).toBe(404);
  });

  it("issues a new OTP for a known phone number", async () => {
    await request(app).post("/auth/signup").send({ name: "Test Driver", phone: TEST_PHONE });

    const response = await request(app).post("/auth/login").send({ phone: TEST_PHONE });

    expect(response.status).toBe(202);
  });
});

describe("POST /auth/verify", () => {
  it("signs up, verifies with the logged OTP, and returns a token", async () => {
    await request(app).post("/auth/signup").send({ name: "Test Driver", phone: TEST_PHONE });
    const code = await readOtpCode(TEST_PHONE);

    const response = await request(app).post("/auth/verify").send({ phone: TEST_PHONE, code });

    expect(response.status).toBe(200);
    expect(response.body.token).toEqual(expect.any(String));
    expect(response.body.user.phone).toBe(TEST_PHONE);
  });

  it("rejects an incorrect code", async () => {
    await request(app).post("/auth/signup").send({ name: "Test Driver", phone: TEST_PHONE });

    const response = await request(app)
      .post("/auth/verify")
      .send({ phone: TEST_PHONE, code: "000000" });

    expect(response.status).toBe(401);
  });
});
