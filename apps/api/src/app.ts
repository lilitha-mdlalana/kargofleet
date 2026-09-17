import express from "express";
import cors from "cors";
import authRoutes from "./routes/auth.routes";
import expensesRoutes from "./routes/expenses.routes";
import profileRoutes from "./routes/profile.routes";

export function createApp() {
  const app = express();

  app.use(cors());
  app.use(express.json());

  app.use("/auth", authRoutes);
  app.use("/expenses", expensesRoutes);
  app.use("/profile", profileRoutes);

  app.get("/health", (_req, res) => {
    res.json({ status: "ok" });
  });

  return app;
}
