create view profile_absorbees as 
with recursive absorbs as (
  select 
    profile_id as absorbee, 
    absorbed_by as profile_id
  from profile
  where absorbed_by is not null
  union all 
  select 
    abso.absorbee, 
    prof.absorbed_by as profile_id
  from absorbs abso
  inner join profile prof
  on prof.profile_id = abso.profile_id
  where prof.absorbed_by is not null
)

select 
absorbs.profile_id,
absorbs.absorbee
from absorbs

;


create view profile_absorber as 
with recursive absorbs as (
  select 
    profile_id, 
    absorbed_by as absorber
  from profile
  where absorbed_by is not null
  union all 
  select 
    abso.profile_id, 
    prof.absorbed_by as absorber
  from absorbs abso
  inner join profile prof
  on prof.profile_id = abso.profile_id
)

select
  profile_id,
  absorber
from absorbs