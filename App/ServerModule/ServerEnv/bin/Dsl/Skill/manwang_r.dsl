skill(105)
{
	section(10)
	{
    hiteffect(hitEffect, "eyes", 0, 1000, "stand", 100);
	};
  section(500)
  {
  	rotate(0, 3000, vector3(0, 1440, 0));
    selfeffect(selfEffect,1000,"eyes",0)
    {
      transform(vector3(0,1,0));
    };
    facetotarget(0,100);
  	enablemoveagent(false);
    charge(500,13,0,vector3(0,0,0),0);
    aoeimpact(500,0,0,3,false);
  };
  section(100)
  {
    animation("stand");
    enablemoveagent(true);
  };
  onstop
  {
    animation("stand");
    enablemoveagent(true);
  };
};