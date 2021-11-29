
--drop database dap_master;

--create database dap_master;

CREATE TABLE project(
	project_id int primary key identity(1,1) not null,
	project_name nvarchar(50) unique not null,
	project_description nvarchar(450) not null,
	created_by int not null,
	created_on datetime2 not null default getdate(),
	last_accessed_on datetime2 not null default getdate(),
	is_favorite bit default 0 not null,
	is_active bit default 1 not null,
	is_deleted bit default 0 not null
);

CREATE TABLE project_user(
	project_id int not null references project,
	[user_id] int not null,
	[permission_bit] bigint not null default 0,
	created_on datetime2 not null default getdate(),
	PRIMARY KEY (project_id, [user_id])
);

CREATE TABLE source_type(
	source_type_id int primary key identity(1,1),
	source_type_name nvarchar(100)
);

insert into source_type(source_type_name)values('FileUpload');
insert into source_type(source_type_name)values('Twitter');
insert into source_type(source_type_name)values('Retsly');
insert into source_type(source_type_name)values('FTP');
insert into source_type(source_type_name)values('S3');


CREATE TABLE writer_type(
	writer_type_id int primary key identity(1,1) not null,
	writer_type_name varchar(50) unique not null,
	created_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null	
);

insert into writer_type(writer_type_name) values('CSV Flat File');
insert into writer_type(writer_type_name) values('Postgres');
insert into writer_type(writer_type_name) values('Mongodb');
insert into writer_type(writer_type_name) values('Elastic');
insert into writer_type(writer_type_name) values('MySql');
insert into writer_type(writer_type_name) values('Oracle');


CREATE TABLE reader_type(
	reader_type_id int primary key identity(1,1) not null,
	reader_type_name varchar(50) unique not null,
	created_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null	
);

insert into reader_type(reader_type_name) values('CSV Reader');
insert into reader_type(reader_type_name) values('Json Reader');
insert into reader_type(reader_type_name) values('Log Reader');

create table writer(
	writer_id int primary key identity(1,1) not null,
	writer_type_id int not null references writer_type,
	[user_id] int not null,	
	destination_path varchar(1000),
	created_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null	
);

insert into writer(writer_type_id, user_id, destination_path) values(2, -1,'postgres conn_str');
insert into writer(writer_type_id, user_id, destination_path) values(3, -1,'mono conn_str');
insert into writer(writer_type_id, user_id, destination_path) values(4, -1,'elastic conn_str');

create table reader(
	reader_id int primary key identity(1,1) not null,
	reader_type_id int not null references reader_type,
	[user_id] int not null,	
	reader_configuration nvarchar(max),
	configuration_name nvarchar(100),
	created_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null	
);

create table project_writer(
	project_id int not null references project,
	writer_id int not null references writer,
	PRIMARY KEY (project_id, writer_id)
);

create table project_reader(
	project_id int not null references project,
	reader_id int not null references reader,
	PRIMARY KEY (project_id, reader_id)
);


create table project_schema(
	[schema_id] int primary key identity(1,1) not null,
	[schema_name] varchar(50) not null, 
	project_id int not null references project,
	type_config varchar(max) not null,
	[user_id] int not null,	
	created_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null		
);

CREATE TABLE project_file(
	project_file_id int primary key identity(1,1),
	project_id int not null references project,
	[user_id] int not null,
	[source_type_id] int not null references source_type,	
	[file_name] varchar(100),
	file_path varchar(255),
	[source_configuration] nvarchar(max),
	upload_date datetime2 not null default getdate(),
	reader_id int references reader,
	[schema_id] int references project_schema,
	is_active bit default 1 not null,
	is_deleted bit default 0 not null
);


create table schema_model(
	model_id int primary key identity(1,1) not null,
	[schema_id] int not null references project_schema,	
	project_id int not null references project,	
	model_name varchar(50),
	model_config nvarchar(max),
	[user_id] int not null,	
	created_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null			
);

create table model_metadata(
	metadata_id int primary key identity(1,1) not null,
	project_id int not null references project,
	model_id int not null references schema_model,
	column_name varchar(50),
	data_type varchar(50),
	created_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null	
);

create table job_status(
	job_status_id int primary key identity(1,1) not null,
	status_name varchar(50),
	created_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null	
);

insert into job_status(status_name) values('Created');
insert into job_status(status_name) values('Running');
insert into job_status(status_name) values('Completed');
insert into job_status(status_name) values('Failed');

create sequence job_sequence as int start with 1  increment by 1;

create table job(
	job_id int not null,
	[project_file_id] int not null references project_file,
	[user_id] int not null,
	job_status_id int not null references job_status,
	[project_id] int not null references project,
	job_description varchar(max),
	[schema_id] int references project_schema,
	created_on datetime2 not null default getdate(),
	started_on datetime2,
	completed_on datetime2,
	is_active bit default 1 not null,
	is_deleted bit default 0 not null,
	PRIMARY KEY (job_id, project_file_id)
);

create table search_history(
	search_history_id int primary key identity(1,1) not null,
	search_history_name nvarchar(50) unique,
	created_on datetime2 not null default getdate(),
	[user_id] int not null,
	project_id int not null references project,
	writer_id int not null references writer,
	search_query nvarchar(max),
	resolved_search_query nvarchar(max),
	md5 nvarchar(50),
	last_executed_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null,
        CONSTRAINT UC_md5 UNIQUE (project_id,md5)
);

create table search_graph(
	search_graph_id int primary key identity(1,1) not null,
	created_on datetime2 not null default getdate(),
	search_history_id int not null references search_history,
	graph_description nvarchar(max),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null
);


create table workflow_server_type(
workflow_server_type_id int primary key not null,
workflow_server_type_name nvarchar(255)
);

insert into workflow_server_type(workflow_server_type_id, workflow_server_type_name) values(1, 'digdag');
insert into workflow_server_type(workflow_server_type_id, workflow_server_type_name) values(2, 'airflow');

create table workflow_project(
workflow_project_id int primary key identity(1,1) not null,
[user_id] int not null,
external_project_id int not null, -- dig dag generated
external_project_name nvarchar(255), -- dig dag project name
created_on datetime2 not null default getdate(),
updated_on datetime2 not null default getdate(),
recent_version_number nvarchar(100), -- most recent version on dig dag
workflow_server_type_id int references workflow_server_type,
[description] nvarchar(255),
is_active bit default 1 not null,
is_deleted bit default 0 not null
 
);

create table workflow_status_type(
workflow_status_type_id int primary key not null,
workflow_status_type_name nvarchar(255) not null
);

insert into workflow_status_type(workflow_status_type_id, workflow_status_type_name) values(1, 'created');
insert into workflow_status_type(workflow_status_type_id, workflow_status_type_name) values(2, 'executing');
insert into workflow_status_type(workflow_status_type_id, workflow_status_type_name) values(3, 'exception');
insert into workflow_status_type(workflow_status_type_id, workflow_status_type_name) values(4, 'completed');

create table workflow_version(
	workflow_version_id int primary key identity(1,1) not null,
	workflow_project_id int not null references workflow_project,
	last_workflow_session_attempt_id int,
	[user_id] int not null,
	external_project_id int not null,
	external_workflow_id int,
	version_number int not null,
	created_on datetime2 not null default getdate(),
	updated_on datetime2 not null default getdate(),
	workflow_json nvarchar(max),
	is_published bit default 0,
	uploaded_path nvarchar(255),
	workflow_property_json nvarchar(max),
	output_model_name nvarchar(255),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null
);

create table workflow_session_attempt(
	workflow_session_attempt_id int primary key identity(1,1) not null,
	workflow_version_id int not null references workflow_version,
	[user_id] int not null,
	workflow_project_id int references workflow_project,
	external_project_id int not null,
	external_workflow_id int not null,
	external_attempt_id int not null,
	version_number int not null,
	created_on datetime2 not null default getdate(),
	end_date datetime2,
	workflow_status_type_id int not null references workflow_status_type,
	workflow_automation_id int,
	result nvarchar(255),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null
);

ALTER TABLE workflow_version
ADD FOREIGN KEY (last_workflow_session_attempt_id) REFERENCES workflow_session_attempt(workflow_session_attempt_id);

create table workflow_session_log(
	workflow_session_log_id int primary key identity(1,1) not null,
	workflow_session_attempt_id int references workflow_session_attempt,
	external_project_id int not null,
	workflow_project_id int references workflow_project,
	version_number int not null,
	created_on datetime2 not null default getdate(),
	log_data nvarchar(max)
);

create table workflow_element(
	workflow_element_id int primary key identity(1,1) not null,
	element_name nvarchar(255) not null,
	element_icon_name nvarchar(255),
	element_properties nvarchar(max),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null
);

create table workflow_search_history(
	workflow_search_history_id int primary key identity(1,1) not null,
	workflow_search_history_name nvarchar(50) unique,
	created_on datetime2 not null default getdate(),
	[user_id] int not null,
	workflow_project_id int not null references workflow_project,
	workflow_version_id int not null references workflow_version,
	search_query nvarchar(max),
	resolved_search_query nvarchar(max),
	md5 nvarchar(50),	
	last_executed_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null,
	 CONSTRAINT WUC_md5 UNIQUE (workflow_project_id,md5)
);

create table project_automation (
 project_automation_id int primary key identity(1,1) not null,
 project_id int not null references project,
 reader_id int not null references reader,
 [project_schema_id] int not null references project_schema,
 folder_path nvarchar(max) not null,
 created_by int not null,
 is_active bit not null default 1,
 is_deleted bit not null default 0
);

create table workflow_search_graph(
	workflow_search_graph_id int primary key identity(1,1) not null,
	created_on datetime2 not null default getdate(),
	workflow_search_history_id int not null references workflow_search_history,
	graph_description nvarchar(max),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null
);

create table workflow_output_model(
	workflow_output_model_id int primary key identity(1,1) not null,	
	workflow_project_id int not null references workflow_project,
	workflow_version_id int not null references workflow_version,
	table_name nvarchar(100),
	display_name nvarchar(100),
	[user_id] int not null,
	created_on datetime2 not null default getdate(),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null
);

create table workflow_model_metadata(
   workflow_model_metadata_id int primary key identity(1,1) not null,
   workflow_version_id int not null  references workflow_version,
   workflow_output_model_id int not null references workflow_output_model,
   column_name nvarchar(255),
   [user_id] int not null,
   created_on datetime2 not null default getdate(),   
   is_active bit default 1 not null,
   is_deleted bit default 0 not null
);

create table workflow_monitor(
   workflow_monitor_id int primary key identity(1,1) not null,
   workflow_project_id int not null references workflow_project,
   workflow_version_id int not null  references workflow_version,
   model_id int references schema_model,
   workflow_output_model_id int references workflow_output_model,
   [user_id] int not null,
   created_on datetime2 not null default getdate(),   
   is_active bit default 1 not null,
   is_deleted bit default 0 not null
);
create table workflow_automation_state(
   workflow_automation_state_id int primary key identity(1,1) not null,
   workflow_version_id int not null  references workflow_version,
   state_status  bit default 1 not null,
   created_on datetime2 not null default getdate(),   
   is_active bit default 1 not null,
   is_deleted bit default 0 not null
);
-- session_id -> job_id in porjects or workflowattemptid in workflow
create table workflow_state_model_map(
   workflow_state_model_map_id int primary key identity(1,1) not null,
   workflow_automation_state_id int not null  references workflow_automation_state,
   workflow_version_id int not null  references workflow_version,
   model_id int references schema_model,
   workflow_output_model_id int  references workflow_output_model,
   session_id int not null,
   created_on datetime2 not null default getdate(),   
   is_active bit default 1 not null,
   is_deleted bit default 0 not null
);
create table workflow_automation(
   workflow_automation_id int primary key identity(1,1) not null,
   workflow_project_id int not null references workflow_project,
   workflow_version_id int not null  references workflow_version,
   workflow_automation_state_id int not null  references workflow_automation_state,
   created_on datetime2 not null default getdate(),   
   is_active bit default 1 not null,
   is_deleted bit default 0 not null
);

create table workflow_test(
	workflow_test_id int primary key identity(1,1) not null,
	workflow_project_id int not null references workflow_project,
	workflow_version_id int not null  references workflow_version,
	[user_id] int not null,
	external_project_id int not null,
	external_workflow_id int,
	created_on datetime2 not null default getdate(),
	updated_on datetime2 not null default getdate(),
	workflow_json nvarchar(max),
	workflow_property_json nvarchar(max),
	external_attempt_id int not null,
	workflow_status_type_id int not null references workflow_status_type,
	result nvarchar(255),
	log_data nvarchar(max),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null
);

create table user_api_key(
    user_api_key_id int primary key identity(1,1) not null,
    [user_id] int not null,
	api_key nvarchar(50),
	created_on datetime2 not null default getdate(),
	updated_on datetime2 not null default getdate(),
	scope  nvarchar(255),
	is_active bit default 1 not null,
	is_deleted bit default 0 not null
);

create table user_api_key_log
(
     user_api_key_log_id int primary key identity(1,1) not null,
	 user_api_key_id int not null,
     [user_id] int not null,
	 accessed_on datetime2 not null default getdate(),
	 accessed_url nvarchar(255),
	 accessed_url_body nvarchar(1000),
	 metadata  nvarchar(1000),
	 is_active bit default 1 not null,
	 is_deleted bit default 0 not null
);
create table user_shared_url
(
      user_shared_url_id int primary key identity(1,1) not null,
	  [user_id] int not null,
	  search_history_id int,
	  workflow_search_history_id int,
	  shared_url nvarchar(255),
	  is_active bit default 1 not null,
	  is_deleted bit default 0 not null
);