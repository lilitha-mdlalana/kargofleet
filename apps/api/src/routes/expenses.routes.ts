import { Router } from "express";
import { listExpenses, createExpense, deleteExpense } from "../controllers/expenses.controller";

const router = Router();

router.get("/", listExpenses);
router.post("/", createExpense);
router.delete("/:id", deleteExpense);

export default router;
