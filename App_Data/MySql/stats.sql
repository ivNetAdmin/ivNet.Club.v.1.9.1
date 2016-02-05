	SELECT bwcc_fixturestats.fixtureid as legacyfixtureid, memberid as legacymemberid, 
	runsscored,battingposition,oversbowled,maidenovers,runsconceeded,wicketstaken,catches,stumpings,
	bwcc_members.Surname as lastname,bwcc_members.Firstname as firstname,bwcc_teams.Name as teamname,bwcc_opposition.Name as opposition, bwcc_howout.Name as howout, bwcc_fixtures.dateplayed
	FROM  bwcc_fixturestats
	INNER JOIN bwcc_members ON bwcc_members.id = bwcc_fixturestats.memberId
	INNER JOIN bwcc_teams ON bwcc_teams.id = bwcc_fixturestats.teamid
	INNER JOIN bwcc_fixtures ON bwcc_fixtures.id = bwcc_fixturestats.fixtureId
	INNER JOIN bwcc_howout ON bwcc_howout.id = bwcc_fixturestats.howoutid
	INNER JOIN bwcc_opposition ON bwcc_opposition.id = bwcc_fixtures.oppositionid
	ORDER BY bwcc_fixturestats.fixtureid, battingposition