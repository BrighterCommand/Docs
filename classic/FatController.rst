Paramore
========

Libraries and supporting examples for use with the Ports and Adapters
and CQRS architectural styles for .NET, with support for Task Queues

`View the Project on GitHub
iancooper/Paramore <https://github.com/iancooper/Paramore>`__

-  `Download **ZIP
   File** <https://github.com/iancooper/Paramore/zipball/master>`__
-  `Download **TAR
   Ball** <https://github.com/iancooper/Paramore/tarball/master>`__
-  `View On **GitHub** <https://github.com/iancooper/Paramore>`__

`Paramore Home <../index.html>`__

`Brighter Home <Brighter.html>`__

`Prev <CommandsCommandDispatcherandProcessor.html>`__

Brighter
========

Fat Controller
--------------

The use of MVC frameworks can lead you into the ‘fat’ controller
anti-pattern. it’s pretty easy to recognize the stink of the fat
controller smell. The controller is hundreds of lines long, packed with
actions, and the actions are long swathes of code, not the handful of
lines that they should be. The two main causes of the stink seem to be:

-  The controller services too many requests (i.e. has too many actions)
-  The controller has domain logic (i.e. model code has crept into the
   controller)

Why is this bad (just in case you could not see that it was)? I’ll
paraphrase the issues identified by Rebecca Wirfs-Brock and Alan McKean
in `Object Design <http://www.wirfs-brock.com/DesignBooks.html>`__ here:

-  Controller logic is overly complex
-  Controllers have become dependent on domain concepts
-  Controllers increase coupling between otherwise distinct entities, as
   they pass information from one entity to another
-  Controllers do the interesting work

All of this can really be summed up as making your code hard to
maintain. Complex logic is easy to write but hard to read. Simple code
requires more effort to write, but pays dividends to maintainers trying
to fix or amend it. Breaking the dependencies between your controller
and the domain make your code much more amenable to change. Need that
domain logic from another action or another controller. It should be
easy because it lives in the domain. Slap it into your controller and
suddenly its harder to get at. Perhaps a new customer needs a different
view of the domain. Perhaps you want to expose the logic in that
controller as an AJAX call instead of on a postback. Perhaps you want to
expose it as a WCF service.easy if lives in the domain, hard if it lives
in the controller.

That kind of pain tends to lead to cut & paste and re-use. Cut & paste
reuse is the road to hell as there are now multiple versions of the
truth.Over time they will deviate and you will have confusion.

The more the controllers know about the domain, the more sticky they
make it. Want to change the domain, well now you have to change the
controllers too. And maybe those UI templates if your domain model was
being directly consumed by the view. And that kind of change is more
expensive because your refactoring tools probably don’t read your
templates.

Skinny Controllers, Fat Model
-----------------------------

For my part, the worst smell comes from the controller becoming
dependent on the domain. The controller begins to hold domain logic,
holding the knowledge of what is is to ship a cargo, purchase a product,
make a payment. The domain objects become aneamic, their only role to
hold data The interaction becomes a controller setting properties on the
domain objects. The controller becomes vampiric, it grows fat by sucking
the life-blood out of your domain, which ends up weak, drained, and
anaemic.

This is the good old problem of domain-logic in code-behind that we had
in ASP.NET webforms. Indeed Webforms are just another type of
controller, a `page
controller <http://martinfowler.com/eaaCatalog/pageController.html>`__,
and switching to an `application
controller <http://martinfowler.com/eaaCatalog/applicationController.html>`__
model does not remove the need for us to watch for domain logic creeping
into the controller. There can be a tempation to believe that just
because the controller is easier to test it is now safe to put domain
logic in there. Do not fall into that trap.

Fighting the Flab
-----------------

Our current solution to this smell is to review our controllers to
ensure that they are doing very little work. We `push responsibility for
co-ordinating between domain
objects <http://devlicio.us/blogs/derik_whittaker/archive/2008/10/22/how-is-interacting-with-your-data-repository-in-your-controller-different-or-better-than-doing-it-in-your-code-behind.aspx>`__
to `domain level
services <http://www.lostechies.com/blogs/jimmy_bogard/archive/2008/08/21/services-in-domain-driven-design.aspx>`__.
The services have no state, they just co-ordinate the actions of domain
objects, such as getting an instance of an entity from the repository
and then calling methods on it. This properly belongs in a service that
co-ordinates that part of our domain, not the controller whose job is to
co-ordinate between the domain (model) and view. 

I suspect, but have no evidence, that a `bottom up (or inside-out) and
not top down (or
outside-in) <http://xunitpatterns.com/Philosophy%20Of%20Test%20Automation.html>`__
strategy to development may also help. If we write the domain entities
and value types before we write the coordinators and controllers we will
tend to find that the functionality tends to cluster at the leaf nodes
of our domain model. If we start at the top then we will tend to find
ourselves putting more logic into the controllers, because we tend to
want to write our tests for something other than just calling a couple
of objects on the next, and as yet only stubbed out, layer below. I
suspect that some of our worse cases of fat controllers may have come
from taking a top-down approach. I suspect this is particularly true
with folks who drive from the UI. Of course the downside with bottom up
or inside-out approaches is because the unit tests drive over the
acceptance tests it is easier to slip in speculative functionality that
turns out not to be needed by the unit test.

The Rails Way is `Skinny Controller, Fat
Model <http://weblog.jamisbuck.org/2006/10/18/skinny-controller-fat-model>`__
and its a good mantra to pick up as you approach development with .NET
MVC applications.

Beware God Controllers
----------------------

The god controller is really just a variation of the `god object
anti-pattern <http://en.wikipedia.org/wiki/God_object>`__. Fat
Controllers have become God Controllers when they take responsibility
for handling too many requests. The pathological case is a site that has
just the one controller which handles all requests from the client.
Whereas the WebForms page controller forced us into a controller per
page, MVC applications don’t force us down this route. So we have to
make sensible decisions about what responsibilities each of our
controllers should have.

| We began by having one controller per aggregate in our domain model.
|  That seemed attractive at first because of the affinity between
|  repositories and aggregates but we have quickly hit overload on our
|  controllers through that approach.

| At the other end we might be think
|  about one controller per entity. This seems attractive at first and
Rails sometimes has a controller per model approach,  but ends up too
granular. It also breaks down because we
|  often have multiple entities on one page.

We could move to having a controller per page. This seems attractive
when coming from a WebForms background but tends to create too many
controllers.Developing with MVC frameworks differs from Webforms in that
there is no support for server-side controls. If you want dynamic
behavior then you need to support it through Javascript and CSS on the
client. When moving to an MVC application you quickly understand that
you need to shift from server side controls to client side controls.
There are many advantages to this. Your template can render simple
clean, standards compliant, HTML. You get clean separation of the UI
widget code away from the controller.

People who have done Javascript in the past shy away at this point
because developing rich interactive applications with Javascript used to
be expensive. It required a lot of low-level coding against the DOM. The
rise of Javascript libraries like JQuery has revolutionized the
development of rich interactive applications because these frameworks
dramatically lower the cost of development.

What it means in this context is that your controller can handle the
code for more than one page very easily. This is true even if you
include the actions that your page will expose to client-side AJAX
calls.

|  A more productive approach looks to be thinking in terms of user
|  activities and provide one controller per user activity. By user
|  activity I mean ordering a product, reviewing an account, checking
out,
|  making a payment. These business transactions seem to be a good
|  granularity level for controllers.

“God is dead. God remains dead. And we have killed him.”
--------------------------------------------------------

| Of course the danger is that the domain service itself becomes
vampiric
|  and drains the life-blood of our domain. Control and co-ordination
are roles that many services fulfill. Our controller, the ‘C’ in MVC can
be thought of as a presentation layer service that co-ordinates between
our view and model. Everything that applies to controllers also applies
to any service that do control and co-ordination. The fat controller is
a specific but not an exclusive case.

| We want to watch for swapping our fat controller
|  for a fat service when we push our code out of the controller and
into
|  a domain service. The mechanism for fighting the flab remains the
same. Figure out what control and co-ordination responsibilities this
service has and do not spread it to include control and co-ordination
responsibilities over other areas. Consider the `Single Responsibility
Principle <http://en.wikipedia.org/wiki/Single_responsibility_principle>`__
as a guide. Break up services to stop
|  them becoming god objects, and push code down into the
|  entities and value types at the same time. I find that sniffing
|  for the `Feature
Envy <http://www.soberit.hut.fi/mmantyla/BadCodeSmellsTaxonomy.htm>`__
|  smells helps uncover the places where refactoring will help push
logic
|  out of the domain level service and into the domain objects.

Keeping Fit
-----------

Development is a bit like life. It is easy to get flabby. We just need
to get lazy and eat too many things that are bad for us. The trouble is
that once you become overweight it gets harder and harder to lose that
weight. So if you don’t want to end up with controllers so overweight
that they require surgical intervention, you need to watch how you code
them and refactor mercilessly once you see those fat deposits
accumulating.

This project is maintained by
`iancooper <https://github.com/iancooper>`__

Hosted on GitHub Pages — Theme by
`orderedlist <https://github.com/orderedlist>`__

