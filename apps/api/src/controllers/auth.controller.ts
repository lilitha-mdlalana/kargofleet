import { Request, Response } from "express";
import { prisma } from "../db/client";
import { AuthError, startLogin, startSignup, verifyOtp } from "../services/auth.service";
import { signToken } from "../services/jwt.service";

export async function signup(req: Request, res: Response): Promise<void> {
  const { name, phone } = req.body ?? {};
  if (typeof name !== "string" || !name.trim() || typeof phone !== "string" || !phone.trim()) {
    res.status(400).json({ message: "name and phone are required" });
    return;
  }
  try {
    await startSignup(prisma, name.trim(), phone.trim());
    res.status(202).json({ message: "OTP sent" });
  } catch (err) {
    if (err instanceof AuthError) {
      res.status(err.status).json({ message: err.message });
      return;
    }
    throw err;
  }
}

export async function login(req: Request, res: Response): Promise<void> {
  const { phone } = req.body ?? {};
  if (typeof phone !== "string" || !phone.trim()) {
    res.status(400).json({ message: "phone is required" });
    return;
  }
  try {
    await startLogin(prisma, phone.trim());
    res.status(202).json({ message: "OTP sent" });
  } catch (err) {
    if (err instanceof AuthError) {
      res.status(err.status).json({ message: err.message });
      return;
    }
    throw err;
  }
}

export async function verify(req: Request, res: Response): Promise<void> {
  const { phone, code } = req.body ?? {};
  if (typeof phone !== "string" || !phone.trim() || typeof code !== "string" || !code.trim()) {
    res.status(400).json({ message: "phone and code are required" });
    return;
  }
  try {
    const user = await verifyOtp(prisma, phone.trim(), code.trim());
    const token = signToken(user.id);
    res.status(200).json({
      token,
      user: { id: user.id, name: user.name, phone: user.phone },
    });
  } catch (err) {
    if (err instanceof AuthError) {
      res.status(err.status).json({ message: err.message });
      return;
    }
    throw err;
  }
}
