import * as admin from 'firebase-admin';
import "reflect-metadata";

admin.initializeApp();

/*
 *
 * HTTPS Functions
 * ------------------------------------------------------------------------------
 *
 *
 */

const https = require("./https/https.functions");
exports.echo = https.echo;
exports.addCustomClaimToUser = https.addCustomClaimToUser;


/*
 *
 * Callable Functions
 * ------------------------------------------------------------------------------
 *
 *
 */

const callables = require("./callables/callables.functions");
exports.convertToLeet = callables.convertToLeet;
exports.triggerNotification = callables.triggerNotification;


/*
 *
 * Triggers : Firestore
 * ------------------------------------------------------------------------------
 *
 *
 */

const triggersFirestore = require("./triggers/firestore/firestore.triggers.functions");
exports.onFirestorePushNotificationCreate = triggersFirestore.onFirestorePushNotificationCreate;