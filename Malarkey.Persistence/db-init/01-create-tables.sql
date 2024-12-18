create table profile (
    profile_id uuid default gen_random_uuid(),
    profile_name varchar(100) not null,
    created_at timestamp default current_timestamp,
    absorbed_by uuid,
    constraint pk_profile primary key (profile_id) 
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
  issued_to varchar(1000) not null,
  issued_at timestamp not null,
  valid_until timestamp not null,
  revoked_at timestamp,
  constraint pk_token primary key (token_id)
)