# Functions

Cloud Functions for Firebase is a serverless framework that lets you automatically run backend code in response to events triggered by Firebase features and HTTPS requests. Your JavaScript or TypeScript code is stored in Google's cloud and runs in a managed environment. There's no need to manage and scale your own servers.

## Setup

- Follow the instructions for the [basic setup](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/README.md#basic-setup)
- Enable Cloud Functions at your project in the [Firebase Console](https://console.firebase.google.com/)
- [Deploy](https://firebase.google.com/docs/functions/get-started?hl=en) your own function
- Initialize CrossFirebase with Functions enabled:

```c#
  CrossFirebase.Initialize(..., new CrossFirebaseSettings(isFunctionsEnabled:true));
```

## Usage

Take a look at the [documentation](https://firebase.google.com/docs/functions/callable?hl=en#call_the_function) for the official Firebase Cloud Function SKDs, because Plugin.Firebase's code is abstracted but still very similar.

Since code should be documenting itself you can also take a look at the following classes:
- [src/.../IFirebaseFunctions.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Functions/IFirebaseFunctions.cs)
- [src/.../IHttpsCallable.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/src/Shared/Functions/IHttpsCallable.cs)
- [tests/.../FunctionsFixture.cs](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/Plugin.Firebase.IntegrationTests/Functions/FunctionsFixture.cs)
- [tests/cloud-functions/.../index.ts](https://github.com/TobiasBuchholz/Plugin.Firebase/blob/master/tests/cloud-functions/functions/src/index.ts)
