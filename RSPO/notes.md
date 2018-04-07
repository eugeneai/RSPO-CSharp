# Точки интереса

 1. C:\projects\BrightstarDB\src\core\BrightstarDB\EntityFramework\Query\SparqlQueryBuilder.cs:289 Variable _graphPatternBuilder. В переменной содержится часть SPARQL-запроса WHERE, и там же проблема с пустой ссылкой.
 2. Там же GetSparqlQuery:251, где тело SELECT формируется из заготовленных кусков.
 3. Ограничения строяться запусками AddTripleConstraint.
