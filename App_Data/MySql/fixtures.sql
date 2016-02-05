SELECT bwcc_fixtures.id as legacyid, dateplayed, bwcc_resulttype.name as resulttype, bwcc_teams.name as team, bwcc_opposition.name as opposition, result, venueid, competitionid
FROM  bwcc_fixtures
INNER JOIN bwcc_teams ON bwcc_teams.id = bwcc_fixtures.teamid 
LEFT OUTER JOIN bwcc_opposition ON bwcc_opposition.id = bwcc_fixtures.oppositionid 
LEFT OUTER JOIN bwcc_resulttype ON bwcc_resulttype.id = bwcc_fixtures.resultid 