import { PrismaClient, User } from "@prisma/client";

const OTP_TTL_MS = 5 * 60 * 1000;

export class AuthError extends Error {
  constructor(
    public status: number,
    message: string,
  ) {
    super(message);
  }
}

export function generateOtpCode(): string {
  return Math.floor(100000 + Math.random() * 900000).toString();
}

type Db = Pick<PrismaClient, "user">;

export async function startSignup(db: Db, name: string, phone: string): Promise<User> {
  const existing = await db.user.findUnique({ where: { phone } });
  if (existing) {
    throw new AuthError(400, "Phone already registered");
  }
  const otpCode = generateOtpCode();
  const otpExpiresAt = new Date(Date.now() + OTP_TTL_MS);
  const user = await db.user.create({
    data: {
      name,
      phone,
      otpCode,
      otpExpiresAt,
      driver: { create: {} },
    },
  });
  console.log(`OTP for ${phone}: ${otpCode}`);
  return user;
}

export async function startLogin(db: Db, phone: string): Promise<User> {
  const existing = await db.user.findUnique({ where: { phone } });
  if (!existing) {
    throw new AuthError(404, "No account with that phone number");
  }
  const otpCode = generateOtpCode();
  const otpExpiresAt = new Date(Date.now() + OTP_TTL_MS);
  const user = await db.user.update({
    where: { phone },
    data: { otpCode, otpExpiresAt },
  });
  console.log(`OTP for ${phone}: ${otpCode}`);
  return user;
}

export async function verifyOtp(db: Db, phone: string, code: string): Promise<User> {
  const user = await db.user.findUnique({ where: { phone } });
  if (!user) {
    throw new AuthError(404, "No account with that phone number");
  }
  const isExpired = !user.otpExpiresAt || user.otpExpiresAt.getTime() < Date.now();
  if (!user.otpCode || user.otpCode !== code || isExpired) {
    throw new AuthError(401, "Invalid or expired code");
  }
  return db.user.update({
    where: { phone },
    data: { otpCode: null, otpExpiresAt: null },
  });
}
