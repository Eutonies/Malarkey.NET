create table profile (
    profile_id uuid default gen_random_uuid(),
    profile_name varchar(100) not null,
    profile_name_uniqueness varchar(100) not null,
    first_name varchar(100),
    last_name varchar(100),
    primary_email varchar(200),
    primary_email_is_verified boolean not null,
    profile_image bytea,
    profile_image_type varchar(50),
    created_at timestamp default current_timestamp,
    absorbed_by uuid,
    constraint pk_profile primary key (profile_id),
    unique(profile_name_uniqueness)
);

create type provider_type as 
enum('Microsoft','Google', 'Facebook', 'Spotify');


create table profile_identity (
    identity_id uuid default gen_random_uuid(),
    profile_id uuid not null,
    provider provider_type not null,
    provider_id varchar(200) not null,
    identity_name varchar(200) not null,
    preferred_name varchar(200),
    middle_names varchar(200),
    last_name varchar(200),
    email varchar(1000),
    constraint pk_profile_identity primary key (identity_id),
    unique(provider, provider_id),
    constraint fk_profile_identity_prof foreign key (profile_id) references profile(profile_id) on delete cascade
);

create index idx_profile_identity_profid on profile_identity(profile_id);


create type token_type as 
enum('Profile','Identity');

create table token (
  token_id uuid default gen_random_uuid(),
  token_type token_type not null,
  profile_id uuid not null,
  identity_id uuid,
  issued_to varchar(2000) not null,
  issued_at timestamp not null,
  valid_until timestamp not null,
  revoked_at timestamp,
  constraint pk_token primary key (token_id)
);



create table authentication_session (
    session_id bigint primary key generated always as identity,
    state uuid default gen_random_uuid(),
    is_internal boolean not null,
    init_time timestamp not null,
    send_to varchar(2000) not null,
    req_send_to varchar(2000),
    req_id_provider provider_type,
    req_state varchar(2000),
    req_scopes varchar(2000),
    authenticated_time timestamp,
    profile_token_id uuid ,
    identity_token_id uuid ,
    audience varchar(2000) not null,
    existing_profile_id uuid,
    always_challenge boolean not null,
    unique(state)
);

create table authentication_session_parameter (
    session_id bigint not null,
    parameter_name_unique varchar(200) not null,
    parameter_name varchar(200) not null,
    parameter_value varchar(2000) not null,
    constraint pk_authentication_session_parameter primary key (session_id, parameter_name_unique),
    constraint fk_authentication_session_parameter_sessid foreign key (session_id) references authentication_session(session_id) on delete cascade
);

create table authentication_idp_session (
    idp_session_id bigint primary key generated always as identity,
    session_id bigint not null,
    id_provider provider_type not null,
    nonce varchar(200),
    code_challenge varchar(2000) not null,
    code_verifier varchar(2000) not null,
    init_time timestamp not null,
    authenticated_time timestamp,
    scopes varchar(2000) not null,
    constraint fk_authentication_idp_session_sessid foreign key (session_id) references authentication_session(session_id) on delete cascade
);


create table id_provider_token (
    token_id bigint primary key generated always as identity,
    identity_id uuid not null,
    token_string varchar(2000) not null,
    issued_at timestamp not null,
    expires_at timestamp not null,
    refresh_token varchar(2000),
    scopes varchar(2000) not null
);

create table profile_email (
    email_address_id bigint primary key generated always as identity,
    profile_id uuid not null,
    email_address_string varchar(200) not null,
    code_string varchar(200) not null,
    last_verification_mail_sent timestamp,
    verified_at timestamp,
    unique(profile_id,email_address_string),
    constraint fk_profile_email_profid foreign key (profile_id) references profile(profile_id) on delete cascade
);

