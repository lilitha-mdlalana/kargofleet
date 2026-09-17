import { Router } from "express";
import { login, signup, verify } from "../controllers/auth.controller";

const router = Router();

router.post("/signup", signup);
router.post("/login", login);
router.post("/verify", verify);

export default router;
