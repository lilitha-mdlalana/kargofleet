import { PrismaClient } from "@prisma/client";
import { PrismaPg } from "@prisma/adapter-pg";

// Prisma 7 requires an explicit driver adapter (the schema `datasource.url`
// field was removed in favor of driver-adapter connections). DATABASE_URL is
// loaded from `.env` by `dotenv/config`, imported at the top of `server.ts`.
const adapter = new PrismaPg({ connectionString: process.env.DATABASE_URL });

export const prisma = new PrismaClient({ adapter });
