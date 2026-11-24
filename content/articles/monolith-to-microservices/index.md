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

**Optimising the database.** Next I went to the database layer: adding proper indexing and normalising some of the tables. It didn't solve the problem. The database layer hadn't been designed correctly in the first place, and that was the root of a lot of our issues. Tuning it couldn't fix that, and every new field or optimisation fought against the original design.

None of these problems is a reason to adopt microservices on its own. But together, and after two honest attempts at fixing things in place, it was clear the architecture was slowing us down more than it was helping. So I started planning how the microservices pattern could fit our case, starting with the single point of failure.

## 1. Creating diagrams of potential microservices

To get a better understanding how it will look, should we commit to microservices, I wanted a visual diagram. So I created it with PlantUML with C4 modeling. This was done over time as new problems appear while maintaining legacy code

## 2. Strangle the monolith, don't rewrite it

We never stopped the world for a rewrite. We used the **Strangler Fig** pattern: put a routing layer in front of the monolith, build the new service next to it, then move traffic over one route at a time. The monolith shrank while the product kept shipping.

Where a new service still had to talk to old code, we put an **anti-corruption layer** between them: a small translation boundary so the monolith's models and naming didn't leak into the new service. It feels like extra work at the time. It's the reason the new services still look clean today.

## 3. Decide how services talk, deliberately

The most important decision after the boundaries is the communication model. The rule of thumb we settled on:

- **Synchronous (HTTP) for queries**, when the caller genuinely needs an answer right now to continue.
- **Asynchronous events for state changes.** "An order was placed" is published once, and any service that cares reacts in its own time. We used <mark class="todo">[Azure Service Bus / RabbitMQ / other]</mark>.

Events bring a classic trap: you save to your database, then publish to the bus, and the process dies in between. The **transactional outbox** pattern fixes this by storing the event in the same database transaction as the state change, then publishing it from a background worker:

```csharp
public async Task PlaceOrderAsync(PlaceOrder command, CancellationToken ct)
{
    var order = Order.Place(command.CustomerId, command.Lines);
    _db.Orders.Add(order);

    // Stored in the same transaction as the order itself,
    // so the event exists if and only if the order does.
    _db.OutboxMessages.Add(OutboxMessage.From(new OrderPlaced(order.Id, order.Total)));

    await _db.SaveChangesAsync(ct);
    // A background worker reads the outbox and publishes to the bus.
}
```

The other half of the deal: **every consumer must be idempotent**. Message brokers deliver at least once, so handling the same event twice has to be harmless.

## 4. Write the decisions down

Every significant choice (a boundary, a broker, a data-ownership rule) went into a short **Architecture Decision Record**: the context, the options, what we chose, and what it costs us. Months later, when someone asks "why on earth is it like this?", the answer is one link away. It also made design reviews faster, because people argued about the trade-offs on paper instead of from memory.

## What went well

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
