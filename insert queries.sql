select * from project_file

select * from project_schema

select * from schema_model

select * from job

insert into project(project_name, project_description, created_by) values('p1','pd1', 1);

insert into project_file(project_id, user_id, source_type_id, file_name, file_path)values(2,1,1,'test.csv','C:\test.csv')
insert into project_file(project_id, user_id, source_type_id, file_name, file_path)values(2,1,1,'test2.csv','C:\test2.csv')


insert into reader(reader_type_id, user_id, reader_configuration, configuration_name) values(1,1,'tt','tt1')

insert into project_reader(project_id, reader_id) values(2,1)

insert into project_schema(schema_name, project_id, type_config, user_id)values('schema1',2,'ee',1)
insert into project_schema(schema_name, project_id, type_config, user_id)values('schema2',2,'ee',1)

insert into schema_model(schema_id, project_id, model_name, model_config,user_id) values(1,2,'schema1_model1','model_config1',1)
insert into schema_model(schema_id, project_id, model_name, model_config,user_id) values(1,2,'schema1_model2','model_config2',1)
insert into schema_model(schema_id, project_id, model_name, model_config,user_id) values(2,2,'schema2_model1','model_config3',1)
insert into schema_model(schema_id, project_id, model_name, model_config,user_id) values(2,2,'schema2_model1','model_config4',1)

update project_file set schema_id = 1, reader_id = 1 where project_file_id = 1
update project_file set schema_id = 2, reader_id = 1 where project_file_id = 2

insert into job (user_id, job_status_id, project_id, project_file_id, schema_id, started_on) values(1,1,2,1,1, CURRENT_TIMESTAMP)
insert into job (user_id, job_status_id, project_id, project_file_id, schema_id, started_on) values(1,1,2,2,2, CURRENT_TIMESTAMP)

