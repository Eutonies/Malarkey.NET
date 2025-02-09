create or replace procedure cleanup_sessions() 
language sql
begin atomic
	delete from authentication_session sess
	where 
	  (sess.init_time < current_timestamp - interval '2 hours' and sess.authenticated_time is null)
	  or
	  (sess.authenticated_time < current_timestamp - interval '1 days' and not exists (
	      select 1 
		  from token tok
		  where tok.token_id in (sess.profile_token_id, sess.identity_token_id)
		  and tok.valid_until > current_timestamp - interval '2 hours'
	
	  ));
end;

create or replace procedure cleanup_tokens() 
language sql
begin atomic
	delete from token tok
	where tok.valid_until < current_timestamp - interval '2 hours';
end;


create or replace procedure cleanup_idp_tokens() 
language sql
begin atomic
	delete from id_provider_token tok
	where tok.expires_at < current_timestamp - interval '2 hours';
end;

create or replace procedure cleanup_profile_emails() 
language sql
begin atomic
	delete from profile_email em
	where not exists (
	  select * 
	  from profile prof
	  where prof.profile_id = em.profile_id
	  and lower(prof.primary_email) = lower(em.email_address_string)
	);
end;

create or replace procedure cleanup() 
language plpgsql
as $$
begin
	call cleanup_sessions();
	call cleanup_tokens();
	call cleanup_idp_tokens();
	call cleanup_profile_emails();
	commit;
end;
$$;


