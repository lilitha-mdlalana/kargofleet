import "dotenv/config";
import express from "express";
import cors from "cors";
import authRoutes from "./routes/auth.routes";
import expensesRoutes from "./routes/expenses.routes";
import profileRoutes from "./routes/profile.routes";

const app = express();
const PORT = process.env.PORT ?? 3000;

app.use(cors());
app.use(express.json());

app.use("/auth", authRoutes);
app.use("/expenses", expensesRoutes);
app.use("/profile", profileRoutes);

app.get("/health", (_req, res) => {
  res.json({ status: "ok" });
});

app.listen(PORT, () => {
  console.log(`Server listening on port ${PORT}`);
});
