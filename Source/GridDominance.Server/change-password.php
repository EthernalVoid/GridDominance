<?php

require 'internals/backend.php';


function run() {
	global $pdo;

	$userid        = getParamUIntOrError('userid');
	$password_old  = getParamPPKOrError('password_old');
	$password_new  = getParamPPKOrError('password_new');
	$appversion    = getParamStrOrError('app_version');

	$signature     = getParamStrOrError('msgk');

	check_commit_signature($signature, [$userid, $password_old, $password_new, $appversion]);

	//----------

	$user = GDUser::QueryOrFail($pdo, $password_old, $userid);

	//----------

	$hash = password_hash($password_new, PASSWORD_BCRYPT);
	if (!$hash) throw new Exception('password_hash failure');

	$stmt = $pdo->prepare("UPDATE users SET password_hash=:pw, last_online=CURRENT_TIMESTAMP(), last_online_app_version=:av WHERE userid=:uid");
	$stmt->bindValue(':uid', $userid, PDO::PARAM_INT);
	$stmt->bindValue(':av', $appversion, PDO::PARAM_STR);
	$stmt->bindValue(':pw', $hash, PDO::PARAM_STR);
	executeOrFail($stmt);

	//----------

	logDebug("password for user $userid changed (v: $appversion)");
	outputResultSuccess(['user' => $user]);
}



try {
	init("change-password");
	run();
} catch (Exception $e) {
	outputErrorException(Errors::INTERNAL_EXCEPTION, 'InternalError', $e, LOGLEVEL::ERROR);
}