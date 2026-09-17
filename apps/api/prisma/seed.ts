import { prisma } from "../src/db/client";

const DEMO_PHONE = "+27821234567";

async function main() {
  const existing = await prisma.user.findUnique({ where: { phone: DEMO_PHONE } });
  if (existing) {
    console.log("Seed data already present, skipping.");
    return;
  }

  const vehicle = await prisma.vehicle.create({
    data: {
      name: "Toyota Quantum",
      registration: "CA 123-456",
    },
  });

  const user = await prisma.user.create({
    data: {
      name: "Thando Mokoena",
      phone: DEMO_PHONE,
      driver: {
        create: {
          license: "C1",
          vehicleId: vehicle.id,
        },
      },
    },
    include: { driver: true },
  });

  await prisma.trip.create({
    data: {
      driverId: user.driver!.id,
      vehicleId: vehicle.id,
      origin: "Cape Town",
      destination: "Stellenbosch",
      status: "scheduled",
    },
  });

  console.log(`Seeded demo driver ${user.name} (${user.phone})`);
}

main()
  .catch((err) => {
    console.error(err);
    process.exitCode = 1;
  });
