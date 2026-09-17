import { describe, expect, it } from "vitest";
import { signToken, verifyToken } from "./jwt.service";

describe("jwt.service", () => {
  it("round-trips a user id through sign and verify", () => {
    const token = signToken("user-123");
    const payload = verifyToken(token);
    expect(payload.sub).toBe("user-123");
  });

  it("throws on an invalid token", () => {
    expect(() => verifyToken("not-a-real-token")).toThrow();
  });
});
