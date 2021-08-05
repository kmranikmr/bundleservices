CREATE SEQUENCE project_seq;

CREATE TABLE project(
	project_id int primary key default nextval ('project_seq') not null,
	project_name varchar(50) unique not null,
	project_description varchar(450) not null,
	created_by int not null,
	created_on timestamp(6) not null default now(),
	last_accessed_on timestamp(6) not null default now(),
	is_favorite boolean default false not null,
	is_active boolean default true not null,
	is_deleted boolean default false not null
);

CREATE TABLE project_user(
	project_id int not null references project,
	user_id int not null,
	permission_bit bigint not null default 0,
	created_on timestamp(6) not null default now(),
	PRIMARY KEY (project_id, user_id)
);

CREATE SEQUENCE source_type_seq;

CREATE TABLE source_type(
	source_type_id int primary key default nextval ('source_type_seq'),
	source_type_name varchar(100)
);

insert into source_type(source_type_name)values('FileUpload');
insert into source_type(source_type_name)values('Twitter');
insert into source_type(source_type_name)values('Retsly');
insert into source_type(source_type_name)values('FTP');
insert into source_type(source_type_name)values('S3');


CREATE SEQUENCE writer_type_seq;

CREATE TABLE writer_type(
	writer_type_id int primary key default nextval ('writer_type_seq') not null,
	writer_type_name varchar(50) unique not null,
	created_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null	
);

insert into writer_type(writer_type_name) values('CSV Flat File');
insert into writer_type(writer_type_name) values('Postgres');
insert into writer_type(writer_type_name) values('Mongodb');
insert into writer_type(writer_type_name) values('Elastic');
insert into writer_type(writer_type_name) values('MySql');
insert into writer_type(writer_type_name) values('Oracle');


CREATE SEQUENCE reader_type_seq;

CREATE TABLE reader_type(
	reader_type_id int primary key default nextval ('reader_type_seq') not null,
	reader_type_name varchar(50) unique not null,
	created_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null	
);

insert into reader_type(reader_type_name) values('CSV Reader');
insert into reader_type(reader_type_name) values('Json Reader');
insert into reader_type(reader_type_name) values('Log Reader');

create sequence writer_seq;

create table writer(
	writer_id int primary key default nextval ('writer_seq') not null,
	writer_type_id int not null references writer_type,
	user_id int not null,	
	destination_path varchar(1000),
	created_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null	
);

insert into writer(writer_type_id, user_id, destination_path) values(2, -1,'postgres conn_str');
insert into writer(writer_type_id, user_id, destination_path) values(3, -1,'mono conn_str');
insert into writer(writer_type_id, user_id, destination_path) values(4, -1,'elastic conn_str');

create sequence reader_seq;

create table reader(
	reader_id int primary key default nextval ('reader_seq') not null,
	reader_type_id int not null references reader_type,
	user_id int not null,	
	reader_configuration text,
	configuration_name varchar(100),
	created_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null	
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


create sequence project_schema_seq;

create table project_schema(
	schema_id int primary key default nextval ('project_schema_seq') not null,
	schema_name varchar(50) not null, 
	project_id int not null references project,
	type_config text not null,
	user_id int not null,	
	created_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null		
);

CREATE SEQUENCE project_file_seq;

CREATE TABLE project_file(
	project_file_id int primary key default nextval ('project_file_seq'),
	project_id int not null references project,
	user_id int not null,
	source_type_id int not null references source_type,	
	file_name varchar(100),
	file_path varchar(255),
	source_configuration text,
	upload_date timestamp(6) not null default now(),
	reader_id int references reader,
	schema_id int references project_schema,
	is_active boolean default true not null,
	is_deleted boolean default false not null
);


create sequence schema_model_seq;

create table schema_model(
	model_id int primary key default nextval ('schema_model_seq') not null,
	schema_id int not null references project_schema,	
	project_id int not null references project,	
	model_name varchar(50),
	model_config text,
	user_id int not null,	
	created_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null			
);

create sequence model_metadata_seq;

create table model_metadata(
	metadata_id int primary key default nextval ('model_metadata_seq') not null,
	project_id int not null references project,
	model_id int not null references schema_model,
	column_name varchar(50),
	data_type varchar(50),
	created_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null	
);

create sequence job_status_seq;

create table job_status(
	job_status_id int primary key default nextval ('job_status_seq') not null,
	status_name varchar(50),
	created_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null	
);

insert into job_status(status_name) values('Created');
insert into job_status(status_name) values('Running');
insert into job_status(status_name) values('Completed');
insert into job_status(status_name) values('Failed');

create sequence job_sequence as int start with 1  increment by 1;

create table job(
	job_id int not null,
	project_file_id int not null references project_file,
	user_id int not null,
	job_status_id int not null references job_status,
	project_id int not null references project,
	job_description text,
	schema_id int references project_schema,
	created_on timestamp(6) not null default now(),
	started_on timestamp(6),
	completed_on timestamp(6),
	is_active boolean default true not null,
	is_deleted boolean default false not null,
	PRIMARY KEY (job_id, project_file_id)
);

create sequence search_history_seq;

create table search_history(
	search_history_id int primary key default nextval ('search_history_seq') not null,
	search_history_name varchar(50) unique,
	created_on timestamp(6) not null default now(),
	user_id int not null,
	project_id int not null references project,
	writer_id int not null references writer,
	search_query text,
	resolved_search_query text,
	md5 varchar(50),
	last_executed_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null,
	CONSTRAINT UC_md5 UNIQUE (project_id,md5)
);

create sequence search_graph_seq;

create table search_graph(
	search_graph_id int primary key default nextval ('search_graph_seq') not null,
	created_on timestamp(6) not null default now(),
	search_history_id int not null references search_history,
	graph_description text,
	is_active boolean default true not null,
	is_deleted boolean default false not null
);


create table workflow_server_type(
workflow_server_type_id int primary key not null,
workflow_server_type_name varchar(255)
);

insert into workflow_server_type(workflow_server_type_id, workflow_server_type_name) values(1, 'digdag');
insert into workflow_server_type(workflow_server_type_id, workflow_server_type_name) values(2, 'airflow');

create sequence workflow_project_seq;

create table workflow_project(
workflow_project_id int primary key default nextval ('workflow_project_seq') not null,
user_id int not null,
external_project_id int not null, -- dig dag generated
external_project_name varchar(255), -- SQLINES DEMO *** ame
created_on timestamp(6) not null default now(),
updated_on timestamp(6) not null default now(),
recent_version_number varchar(100), -- SQLINES DEMO *** on on dig dag
workflow_server_type_id int references workflow_server_type,
description varchar(255),
is_active boolean default true not null,
is_deleted boolean default false not null
 
);

create table workflow_status_type(
workflow_status_type_id int primary key not null,
workflow_status_type_name varchar(255) not null
);

insert into workflow_status_type(workflow_status_type_id, workflow_status_type_name) values(1, 'created');
insert into workflow_status_type(workflow_status_type_id, workflow_status_type_name) values(2, 'executing');
insert into workflow_status_type(workflow_status_type_id, workflow_status_type_name) values(3, 'exception');
insert into workflow_status_type(workflow_status_type_id, workflow_status_type_name) values(4, 'completed');

create sequence workflow_version_seq;

create table workflow_version(
	workflow_version_id int primary key default nextval ('workflow_version_seq') not null,
	workflow_project_id int not null references workflow_project,
	last_workflow_session_attempt_id int,
	user_id int not null,
	external_project_id int not null,
	external_workflow_id int,
	version_number int not null,
	created_on timestamp(6) not null default now(),
	updated_on timestamp(6) not null default now(),
	workflow_json text,
	is_published boolean default false,
	uploaded_path varchar(255),
	workflow_property_json text,
	output_model_name varchar(255),
	is_active boolean default true not null,
	is_deleted boolean default false not null
);

create sequence workflow_session_attempt_seq;

create table workflow_session_attempt(
	workflow_session_attempt_id int primary key default nextval ('workflow_session_attempt_seq') not null,
	workflow_version_id int not null references workflow_version,
	user_id int not null,
	workflow_project_id int references workflow_project,
	external_project_id int not null,
	external_workflow_id int not null,
	external_attempt_id int not null,
	version_number int not null,
	created_on timestamp(6) not null default now(),
	end_date timestamp(6),
	workflow_status_type_id int not null references workflow_status_type,
	workflow_automation_id int,
	result varchar(255),
	is_active boolean default true not null,
	is_deleted boolean default false not null
);

ALTER TABLE workflow_version
ADD FOREIGN KEY (last_workflow_session_attempt_id) REFERENCES workflow_session_attempt(workflow_session_attempt_id);

create sequence workflow_session_log_seq;

create table workflow_session_log(
	workflow_session_log_id int primary key default nextval ('workflow_session_log_seq') not null,
	workflow_session_attempt_id int references workflow_session_attempt,
	external_project_id int not null,
	workflow_project_id int references workflow_project,
	version_number int not null,
	created_on timestamp(6) not null default now(),
	log_data text
);

create sequence workflow_element_seq;

create table workflow_element(
	workflow_element_id int primary key default nextval ('workflow_element_seq') not null,
	element_name varchar(255) not null,
	element_icon_name varchar(255),
	element_properties text,
	is_active boolean default true not null,
	is_deleted boolean default false not null
);

create sequence workflow_search_history_seq;

create table workflow_search_history(
	workflow_search_history_id int primary key default nextval ('workflow_search_history_seq') not null,
	workflow_search_history_name varchar(50) unique,
	created_on timestamp(6) not null default now(),
	user_id int not null,
	workflow_project_id int not null references workflow_project,
	workflow_version_id int not null references workflow_version,
	search_query text,
	resolved_search_query text,
	md5 varchar(50) unique,	
	last_executed_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null
);

create sequence project_automation_seq;

create table project_automation (
 project_automation_id int primary key default nextval ('project_automation_seq') not null,
 project_id int not null references project,
 reader_id int not null references reader,
 project_schema_id int not null references project_schema,
 folder_path text not null,
 created_by int not null,
 is_active boolean not null default true,
 is_deleted boolean not null default false
);

create sequence workflow_search_graph_seq;

create table workflow_search_graph(
	workflow_search_graph_id int primary key default nextval ('workflow_search_graph_seq') not null,
	created_on timestamp(6) not null default now(),
	workflow_search_history_id int not null references workflow_search_history,
	graph_description text,
	is_active boolean default true not null,
	is_deleted boolean default false not null
);

create sequence workflow_output_model_seq;

create table workflow_output_model(
	workflow_output_model_id int primary key default nextval ('workflow_output_model_seq') not null,	
	workflow_project_id int not null references workflow_project,
	workflow_version_id int not null references workflow_version,
	table_name varchar(100),
	display_name varchar(100),
	user_id int not null,
	created_on timestamp(6) not null default now(),
	is_active boolean default true not null,
	is_deleted boolean default false not null
);

create sequence workflow_model_metadata_seq;

create table workflow_model_metadata(
   workflow_model_metadata_id int primary key default nextval ('workflow_model_metadata_seq') not null,
   workflow_version_id int not null  references workflow_version,
   workflow_output_model_id int not null references workflow_output_model,
   column_name varchar(255),
   user_id int not null,
   created_on timestamp(6) not null default now(),   
   is_active boolean default true not null,
   is_deleted boolean default false not null
);

create sequence workflow_monitor_seq;

create table workflow_monitor(
   workflow_monitor_id int primary key default nextval ('workflow_monitor_seq') not null,
   workflow_project_id int not null references workflow_project,
   workflow_version_id int not null  references workflow_version,
   model_id int references schema_model,
   workflow_output_model_id int references workflow_output_model,
   user_id int not null,
   created_on timestamp(6) not null default now(),   
   is_active boolean default true not null,
   is_deleted boolean default false not null
);
create sequence workflow_automation_state_seq;

create table workflow_automation_state(
   workflow_automation_state_id int primary key default nextval ('workflow_automation_state_seq') not null,
   workflow_version_id int not null  references workflow_version,
   state_status  boolean default true not null,
   created_on timestamp(6) not null default now(),   
   is_active boolean default true not null,
   is_deleted boolean default false not null
);
-- SQLINES DEMO *** _id in porjects or workflowattemptid in workflow
create sequence workflow_state_model_map_seq;

create table workflow_state_model_map(
   workflow_state_model_map_id int primary key default nextval ('workflow_state_model_map_seq') not null,
   workflow_automation_state_id int not null  references workflow_automation_state,
   workflow_version_id int not null  references workflow_version,
   model_id int references schema_model,
   workflow_output_model_id int  references workflow_output_model,
   session_id int not null,
   created_on timestamp(6) not null default now(),   
   is_active boolean default true not null,
   is_deleted boolean default false not null
);
create sequence workflow_automation_seq;

create table workflow_automation(
   workflow_automation_id int primary key default nextval ('workflow_automation_seq') not null,
   workflow_project_id int not null references workflow_project,
   workflow_version_id int not null  references workflow_version,
   workflow_automation_state_id int not null  references workflow_automation_state,
   created_on timestamp(6) not null default now(),   
   is_active boolean default true not null,
   is_deleted boolean default false not null
);

create sequence workflow_test_seq;

create table workflow_test(
	workflow_test_id int primary key default nextval ('workflow_test_seq') not null,
	workflow_project_id int not null references workflow_project,
	workflow_version_id int not null  references workflow_version,
	user_id int not null,
	external_project_id int not null,
	external_workflow_id int,
	created_on timestamp(6) not null default now(),
	updated_on timestamp(6) not null default now(),
	workflow_json text,
	workflow_property_json text,
	external_attempt_id int not null,
	workflow_status_type_id int not null references workflow_status_type,
	result varchar(255),
	log_data text,
	is_active boolean default true not null,
	is_deleted boolean default false not null
);

create sequence user_api_key_seq;

create table user_api_key(
    user_api_key_id int primary key default nextval ('user_api_key_seq') not null,
    user_id int not null,
	api_key varchar(50),
	created_on timestamp(6) not null default now(),
	updated_on timestamp(6) not null default now(),
	scope  varchar(255),
	is_active boolean default true not null,
	is_deleted boolean default false not null
);

create sequence user_api_key_log_seq;

create table user_api_key_log
(
     user_api_key_log_id int primary key default nextval ('user_api_key_log_seq') not null,
	 user_api_key_id int not null,
     user_id int not null,
	 accessed_on timestamp(6) not null default now(),
	 accessed_url varchar(255),
	 accessed_url_body varchar(1000),
	 metadata  varchar(1000),
	 is_active boolean default true not null,
	 is_deleted boolean default false not null
);
create sequence user_shared_url_seq;

create table user_shared_url
(
      user_shared_url_id int primary key default nextval ('user_shared_url_seq') not null,
	  user_id int not null,
	  search_history_id int,
	  workflow_search_history_id int,
	  shared_url varchar(255),
	  is_active boolean default true not null,
	  is_deleted boolean default false not null
);