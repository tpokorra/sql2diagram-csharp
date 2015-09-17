SQL2Diagram for C#
==================

This is a port of [sql2diagram](https://github.com/tpokorra/sql2diagram/) that was originally written in C++, to C#.

Still missing
-------------
* Writing dia files
* producing HTML maps for diagram pngs

Testing
-------
see https://github.com/openpetra/openpetra/blob/master/db/database.build the dbdoc task

sample calls to produce output similar to https://dbdoc.openpetra.org/index.html?table=p_language&group=common:

```
mono sql2diagram.exe -d -f /home/timotheus/dev/openpetra/setup/petra0300/petra.sql > master_alltables.prj
mono sql2diagram.exe -g -f /home/timotheus/dev/openpetra/setup/petra0300/petra.sql > master_alltables_bygroup.prj
mono sql2diagram.exe -f /home/timotheus/dev/openpetra/setup/petra0300/petra.sql -p /home/timotheus/dev/openpetra//db/doc/themed.prj 
```
