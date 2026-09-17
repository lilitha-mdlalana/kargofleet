import { describe, expect, it, vi } from "vitest";
import { signToken } from "../services/jwt.service";
import { requireAuth } from "./auth.middleware";

function fakeRes() {
  const res: any = {};
  res.status = vi.fn().mockReturnValue(res);
  res.json = vi.fn().mockReturnValue(res);
  return res;
}

describe("requireAuth", () => {
  it("calls next and sets userId for a valid token", () => {
    const token = signToken("user-1");
    const req: any = { headers: { authorization: `Bearer ${token}` } };
    const res = fakeRes();
    const next = vi.fn();

    requireAuth(req, res, next);

    expect(next).toHaveBeenCalledTimes(1);
    expect(req.userId).toBe("user-1");
  });

  it("rejects a missing authorization header", () => {
    const req: any = { headers: {} };
    const res = fakeRes();
    const next = vi.fn();

    requireAuth(req, res, next);

    expect(next).not.toHaveBeenCalled();
    expect(res.status).toHaveBeenCalledWith(401);
  });

  it("rejects a malformed token", () => {
    const req: any = { headers: { authorization: "Bearer not-a-token" } };
    const res = fakeRes();
    const next = vi.fn();

    requireAuth(req, res, next);

    expect(next).not.toHaveBeenCalled();
    expect(res.status).toHaveBeenCalledWith(401);
  });
});
