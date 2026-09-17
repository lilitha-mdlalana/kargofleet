import { describe, expect, it, vi } from "vitest";
import { AuthError, startLogin, startSignup, verifyOtp } from "./auth.service";

function fakeDb(overrides: { findUnique?: any; create?: any; update?: any } = {}) {
  return {
    user: {
      findUnique: overrides.findUnique ?? vi.fn(),
      create: overrides.create ?? vi.fn(),
      update: overrides.update ?? vi.fn(),
    },
  } as any;
}

describe("startSignup", () => {
  it("creates a user with a 6-digit OTP when the phone is new", async () => {
    const create = vi.fn().mockResolvedValue({ id: "u1", phone: "+27821112222", name: "Ada" });
    const db = fakeDb({ findUnique: vi.fn().mockResolvedValue(null), create });

    await startSignup(db, "Ada", "+27821112222");

    expect(create).toHaveBeenCalledTimes(1);
    const args = create.mock.calls[0][0];
    expect(args.data.name).toBe("Ada");
    expect(args.data.otpCode).toMatch(/^\d{6}$/);
  });

  it("rejects a phone that is already registered", async () => {
    const db = fakeDb({ findUnique: vi.fn().mockResolvedValue({ id: "u1" }) });

    await expect(startSignup(db, "Ada", "+27821112222")).rejects.toThrow(AuthError);
  });
});

describe("startLogin", () => {
  it("rejects an unknown phone number", async () => {
    const db = fakeDb({ findUnique: vi.fn().mockResolvedValue(null) });

    await expect(startLogin(db, "+27821112222")).rejects.toThrow(AuthError);
  });

  it("regenerates an OTP for a known phone number", async () => {
    const update = vi.fn().mockResolvedValue({ id: "u1" });
    const db = fakeDb({ findUnique: vi.fn().mockResolvedValue({ id: "u1" }), update });

    await startLogin(db, "+27821112222");

    expect(update).toHaveBeenCalledTimes(1);
    expect(update.mock.calls[0][0].data.otpCode).toMatch(/^\d{6}$/);
  });
});

describe("verifyOtp", () => {
  it("succeeds with a matching, unexpired code and clears it", async () => {
    const update = vi.fn().mockResolvedValue({ id: "u1", phone: "+27821112222" });
    const db = fakeDb({
      findUnique: vi.fn().mockResolvedValue({
        id: "u1",
        phone: "+27821112222",
        otpCode: "123456",
        otpExpiresAt: new Date(Date.now() + 60_000),
      }),
      update,
    });

    const user = await verifyOtp(db, "+27821112222", "123456");

    expect(user.id).toBe("u1");
    expect(update.mock.calls[0][0].data).toEqual({ otpCode: null, otpExpiresAt: null });
  });

  it("rejects a wrong code", async () => {
    const db = fakeDb({
      findUnique: vi.fn().mockResolvedValue({
        otpCode: "123456",
        otpExpiresAt: new Date(Date.now() + 60_000),
      }),
    });

    await expect(verifyOtp(db, "+27821112222", "000000")).rejects.toThrow(AuthError);
  });

  it("rejects an expired code", async () => {
    const db = fakeDb({
      findUnique: vi.fn().mockResolvedValue({
        otpCode: "123456",
        otpExpiresAt: new Date(Date.now() - 1000),
      }),
    });

    await expect(verifyOtp(db, "+27821112222", "123456")).rejects.toThrow(AuthError);
  });
});
