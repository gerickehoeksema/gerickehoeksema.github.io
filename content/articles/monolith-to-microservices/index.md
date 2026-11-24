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

The product multiple web interfaces but a single ASP.NET Framework 4.8 API application with one shared database. It had grown for over many years and been through multiple dev houses, and it showed the usual symptoms:

- **Every change shipped everything.** A small fix in one area meant redeploying the whole system, so releases got bigger and longer.
- **The database layer.** Modules read and wrote each other's tables directly, so nobody could change a schema with confidence and ORM layer had no clear boundaries.
- **Extendibility issues.** New features meant spending more hours on connecting layers than actually building it.
- **Updating modules and packages.** 

None of these is a reason to adopt microservices on its own. Together, they meant the architecture was now slowing the team down more than it was helping.

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
