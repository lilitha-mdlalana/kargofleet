import { Request, Response } from "express";

export function listExpenses(req: Request, res: Response) {
  res.status(501).json({ message: "not implemented" });
}

export function createExpense(req: Request, res: Response) {
  res.status(501).json({ message: "not implemented" });
}

export function deleteExpense(req: Request, res: Response) {
  res.status(501).json({ message: "not implemented" });
}
