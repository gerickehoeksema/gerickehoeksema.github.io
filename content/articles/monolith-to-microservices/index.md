---
title: "Breaking up a monolith: what I'd keep, and what I'd do differently"
description: "Lessons from leading a monolith-to-microservices migration: drawing service boundaries, choosing how services talk, and the things I'd change next time."
lede: "I led the move from a monolith to independently deployable services. Here's how we drew the lines, how the services ended up talking to each other, and the things I'd change if I did it again tomorrow."
kicker: Architecture
icon: boxes
date: 2026-12-01
lens: [work]
visibility: draft          # public | unlisted | draft
keywords: [microservices, software architecture, .NET, Azure, monolith decomposition]
---

## Where we started

The product had multiple web applications, all integrating with a single ASP.NET Framework 4.8 API backed by one shared database. The API had grown over many years and passed through several dev houses, and it showed.

The biggest problem was simple: **the API was a single point of failure.** Every web application depended on it, so when the API went down, every application stopped working with it.

And it did go down. Design issues and memory leaks meant that the more traffic we got, the worse the API performed, until it shut down altogether. That happened at various times, and it hit hardest when the company was busiest: exactly when the system was needed most, it was most likely to fall over.

Behind that sat the usual symptoms of a system that had outgrown its design:

- **Every change shipped everything.** A small fix in one area meant redeploying the whole system, so releases got bigger and longer.
- **The database layer.** Modules read and wrote each other's tables directly, and the ORM layer had no clear boundaries, so nobody could change a schema with confidence. Even adding a new field was harder than it should have been.
- **Extendibility issues.** New features meant spending more hours connecting layers than actually building the feature.
- **Updating modules and packages.** <mark class="todo">[What made upgrades hard? E.g. outdated packages tied to .NET Framework, or one upgrade forcing changes across the whole API.]</mark>

## What we tried first

Microservices weren't the first answer. Before changing the architecture, I tried to fix what we had.

**Fixing the memory leaks.** This was the obvious place to start, but in a legacy codebase of that size it turned into such a hassle that I had to abandon it. It simply wasn't feasible time-wise, while the client still needed new features delivered.

**Optimising the database.** Next I went to the database layer: adding proper indexing and normalising some of the tables. It didn't solve the problem. The database layer hadn't been designed correctly in the first place, and that was the root of a lot of our issues. Tuning it couldn't fix that, and every new field or optimisation fought against the original design, so I abandoned reworking the database layer.

**Upgrading the Azure Functions.** That surfaced the next problem. Some applications used Azure Functions to interact with the database, and those functions ran on a model that was no longer supported, so they had to be upgraded. <mark class="todo">[Confirm the model, e.g. the in-process model moving to the isolated worker model, and the .NET versions involved.]</mark> But multiple functions shared the same codebase, so upgrading one meant upgrading all the others. Because they were so tightly coupled, every upgrade caused breaking changes somewhere else. After a lot of time spent there as well, it was clear I couldn't fix it without doing a full rewrite.

None of these problems is a reason to adopt microservices on its own. But together, and after two attempts at fixing the database layer that didn't help at all, it was clear the architecture was slowing us down more than it was helping. So I started planning how the microservices pattern could fit our case, starting with the single point of failure.

## 1. A failing app becomes the first service

After the attempts at fixing the legacy system, the client came with a new request: an existing application had to be fixed and extended in the same piece of work. It was a legacy mobile app that talked to the legacy API, and once again the problem was the foundation underneath it. Through that API it still depended on the existing database, it was in a poor state, and it simply didn't work. Its main process timed out after 30 minutes without ever completing.

Instead of trying to fix the legacy app, I used the **Strangler Fig** pattern: leave the legacy app and API where they are, and build a new application next to them, designed as microservices from the start. That new application would replace the old one and become the foundation for our move to microservices.

## 2. Creating diagrams of potential microservices

To get a better understanding of how it would look, should we commit to microservices, I wanted a visual diagram. So I created one with PlantUML, using the C4 model. This happened over time, as new problems appeared while maintaining the legacy code, and those diagrams turned out to be useful for far more than planning, as you'll see in the next section.

## 3. Strangle the monolith, but don't port the mess

We never stopped the world for a big-bang rewrite of the whole system. The Strangler Fig pattern lets you grow the new system around the old one, and in our case that didn't need a routing layer or any clever traffic splitting. The application boundary did the work: the new application was built alongside the legacy app, the legacy app and API kept running untouched for everything else, and once the new application was ready it replaced the old one. One piece of functionality moved off the legacy API without the rest of the system having to change.

That doesn't mean the code inside the new service should be the old code moved over. My first approach was to scope the legacy code to find the business rules, copy the existing code across, and adapt it for microservices. It didn't work. The business rules themselves weren't implemented correctly. There were no clear boundaries and no clear design, and the code was tightly coupled to a lot of other functionality the process didn't need. From what I could tell, that was the cause of the timeout: the legacy service was basically calling itself multiple times to complete one process.

After a few attempts, and more wasted time, I went back to the client and was honest: this is a big mess. I presented my findings using the diagrams, then stepped back and recommended a rewrite of this part of the system. We would gather the business rules, gather the new features that were going to be introduced, and build it cleanly. It would be faster to rewrite than to copy the existing code and try to fix it.

The lesson: **strangle the system, rewrite the slice.** The Strangler Fig pattern protects you from rewriting everything at once. It doesn't oblige you to carry the old code's problems into the new service.

## 4. My first mistake: going low-code

With the new application, my first instinct was to go for a low-code option. We were already on Azure, so I wanted to use **Azure Logic Apps**. Logic Apps really are great, and they're a great way to debug failures: every run records what happened at each step, so you can see exactly where something broke.

The mistake was that I didn't take into account how big the payloads would actually be for certain parts of the application. My first iteration used a Logic App on the low-cost tier, and it couldn't handle the payload. <mark class="todo">[Which tier and which limit did you hit? E.g. Consumption plan, message size or timeout.]</mark> Making it work meant upgrading to a more expensive option.

That ran straight into a cost trade-off we'd already been weighing. We knew from the start that microservices would push the monthly bill up: every service that owns its own database is another database to pay for. We accepted that because faster feature delivery and easier debugging would cut development costs. Those two costs were always balancing against each other, and paying for a bigger Logic App tier on top of the extra infrastructure tipped the balance the wrong way. At that time, the upgrade didn't make sense.

The lesson: **size the payload before you pick the platform.** Low-code is a good fit for orchestration and glue, but check its limits against your real data first, and check what the next tier up costs before you depend on it.

## 5. A simpler architecture

After dropping the Logic App, we went with a simpler shape. The front end has its own back end: an ASP.NET Core API with its own SQL database, accessed through an Entity Framework Core data layer. Initially, the front end talked to that API directly over HTTPS. <mark class="todo">[Is "its own back end" a Backend for Frontend (one API per front end)? If so, it's worth naming the pattern here.]</mark>

## 6. Solving the timeout: accept, queue, process

With the architecture up and running, it was time to tackle the problems the new application was built to solve. The big one was the legacy app's timeout: a long process with no responsiveness, which failed after 30 minutes without completing.

The fix was to stop making the front end wait. I used the **transactional outbox** pattern together with .NET's built-in **channels** (`System.Threading.Channels`), with one channel per request type inside the API. Each type of work gets its own queue and its own worker, so a flood of one kind of request doesn't hold up the others.

The flow looks like this:

1. **The front end posts the payload and doesn't wait.** It immediately gets a response saying the process has been queued.
2. **The API stores the request in the outbox,** in the same database transaction, so it survives even if the process dies right after.
3. **The request goes into the channel for its type,** where a background worker picks it up and runs the business logic.
4. **If processing fails, a retry service requeues it.** Nothing is lost, and nobody has to resubmit.

A simplified version in C#, showing one request type:

```csharp
// The endpoint stores the request, queues it, and returns straight away.
app.MapPost("/requests", async (ProcessRequest request, AppDbContext db,
    Channel<Guid> queue, CancellationToken ct) =>
{
    var message = OutboxMessage.From(request);
    db.OutboxMessages.Add(message);
    await db.SaveChangesAsync(ct);   // durable before we say "queued"

    await queue.Writer.WriteAsync(message.Id, ct);
    return Results.Accepted($"/requests/{message.Id}", new { message.Id, Status = "Queued" });
});

// A background worker reads the channel and runs the business logic.
public sealed class RequestWorker(Channel<Guid> queue, IServiceScopeFactory scopes)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var id in queue.Reader.ReadAllAsync(ct))
        {
            using var scope = scopes.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<RequestProcessor>();

            // Marks the outbox row as done, or as failed so the
            // retry service can put it back on the channel.
            await processor.ProcessAsync(id, ct);
        }
    }
}
```

The catch with retries: **processing must be idempotent.** If a request can be requeued, running it twice has to be harmless.

The result: a process that used to fail after 30 minutes now completes in **under 3 minutes, even for a massive payload.**

## 7. Write the decisions down

Every significant choice (a boundary, a broker, a data-ownership rule) went into a short **Architecture Decision Record**: the context, the options, what we chose, and what it costs us. Months later, when someone asks "why on earth is it like this?", the answer is one link away. It also made design reviews faster, because people argued about the trade-offs on paper instead of from memory.

## Beyond the timeout: the other drivers

Fixing one failing app was the trigger, but it wasn't the only reason to change the architecture. Working on the new application exposed several other problems with the legacy setup.

### Security risks

The first security risk surfaced quickly. The legacy endpoint the new application needed was open, and because the application had to integrate with third-party applications, that open endpoint exposed a lot of sensitive information. Securing it couldn't be done from one side only: the endpoint had to be secured from the front-end perspective and from the back-end perspective. <mark class="todo">[How did you secure each side? E.g. authentication on the front end, Entra ID / OAuth tokens, API keys or scopes for third parties, Azure API Management, private endpoints.]</mark>

### Infrastructure cost vs development cost

<mark class="todo">[Your note to come. Section 4's Logic App story is one example of this trade-off.]</mark>

### Fast turnaround

<mark class="todo">[Your note to come.]</mark>

### New developer onboarding

<mark class="todo">[Your note to come.]</mark>

## What went well

- **A solid foundation, built for extension.** Because this was our first microservice, we could focus from day one on design patterns that make the system easy to extend, and stay true to SOLID principles instead of fighting years of legacy decisions. <mark class="todo">[Name one or two patterns you used, e.g. on the API or the front end.]</mark>
- **Room to experiment.** Starting fresh meant we could try out design patterns properly, even on the front end. It was genuinely fun, which isn't something I'd have said about working on the legacy API.
- **New features are simpler, even today.** Extending the application or introducing new features is nowhere near as complicated as it used to be.
- **A timeout turned into minutes.** The process that failed after 30 minutes on the legacy app now completes in under 3 minutes, even for a massive payload.

<mark class="todo">[The bullets below are from the earlier draft. Keep the ones that are true for you.]</mark>

- **Independent releases.** <mark class="todo">[A before/after: release frequency, lead time, or simply "a fix in X no longer waits for Y".]</mark>
- **Clearer ownership.** Each service has a team that knows it, and code reviews got sharper because the scope was smaller.
- **Safer change.** Owning its data means a service can change its schema without a cross-team meeting.

## What I'd do differently

This is the part I most wanted to write. <mark class="todo">[Keep the points that are true for you, cut the rest, and add your own.]</mark>

- **Build observability first, not last.** Correlation IDs, distributed tracing (OpenTelemetry into Application Insights) and structured logs should exist before the second service does. Debugging a request across services without them is guesswork.
- **Start with a modular monolith.** Enforcing module boundaries *inside* the monolith first, with separate schemas and no cross-module table access, would have shown us the right seams before we paid for network calls.
- **Make services fewer and bigger at first.** It's much easier to split a service that has grown too big than to merge two that are too chatty.
- **Don't share domain libraries.** A shared "common models" package quietly couples every service to every other one. Share contracts (message and API schemas), not domain classes.
- **Invest in the paved road.** Templates for a new service (pipeline, health checks, logging, config) pay for themselves by the third service.

## A checklist before you split

{.checklist}
1. Can you name the business capability each service owns, in one sentence?
2. Does each service own its data, with no shared tables?
3. Is there one team accountable for each service?
4. Have you chosen, per interaction, between a synchronous call and an event?
5. Are events published reliably (outbox) and consumed idempotently?
6. Can you trace one request across every service it touches?
7. Are the big decisions written down as ADRs?

If most of these are "not yet", a modular monolith is probably the better next step, and that's a perfectly good place to be.

{.article-cta}
I'd love to hear how other teams approached this. What would you add to the list? Let me know on [LinkedIn](https://www.linkedin.com/in/gericke-hoeksema-b132997b/).
