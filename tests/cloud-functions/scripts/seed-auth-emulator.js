#!/usr/bin/env node

const admin = require("../functions/node_modules/firebase-admin");

const projectId = process.env.GCLOUD_PROJECT || "demo-pluginfirebase-integrationtests";
process.env.FIREBASE_AUTH_EMULATOR_HOST =
  process.env.FIREBASE_AUTH_EMULATOR_HOST || "127.0.0.1:9099";

const knownEmails = [
  "created-user@test.com",
  "sign-in-with-pw@test.com",
  "does-not-exist@test.com",
  "sign-out@test.com",
  "to-update-email@test.com",
  "to-update-pw@test.com",
  "to-update-profile@test.com",
  "verification-email@test.com",
  "reload-current-user@test.com",
  "set-language-code@test.com",
  "to-delete@test.com",
  "custom-claims@test.com",
];

admin.initializeApp({ projectId });

async function deleteIfExists(email) {
  try {
    const user = await admin.auth().getUserByEmail(email);
    await admin.auth().deleteUser(user.uid);
    console.log(`[auth seed] deleted ${email}`);
  } catch (error) {
    if (error.code !== "auth/user-not-found") {
      throw error;
    }
  }
}

async function main() {
  for (const email of knownEmails) {
    await deleteIfExists(email);
  }

  const customClaimsUser = await admin.auth().createUser({
    email: "custom-claims@test.com",
    password: "123456",
    emailVerified: true,
  });
  await admin
    .auth()
    .setCustomUserClaims(customClaimsUser.uid, {
      is_awesome: true,
      nested_object: {
        enabled: true,
        roles: ["admin", "tester"],
        metadata: {
          source: "emulator",
          version: 2,
        },
        history: [
          { action: "created", count: 1 },
          { action: "updated", count: 2 },
        ],
        score: 7,
        ratio: 1.5,
        optional: null,
      },
      nested_array: [
        { name: "first", flags: [true, false] },
        { name: "second", metadata: { source: "emulator" } },
      ],
    });
  console.log("[auth seed] recreated custom-claims@test.com");
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
