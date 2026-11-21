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

Most monolith-to-microservices stories start with a diagram of tidy little boxes. Ours started with a deployment that needed <mark class="todo">[how long / how many people / how much coordination]</mark> and a team that had learned to be nervous on release day.

This isn't a "microservices good, monolith bad" post. A well-structured monolith is a perfectly good architecture, and for many teams it's the right one. This is about what happens when the monolith stops fitting the team and the product, and the decisions that mattered most when we split it up.

## Where we started

The product was a single <mark class="todo">[ASP.NET / .NET version]</mark> application with one shared database. It had grown for <mark class="todo">[N]</mark> years, and it showed the usual symptoms:

- **Every change shipped everything.** A small fix in one area meant redeploying the whole system, so releases got bigger and rarer.
- **The database was the real integration layer.** Modules read and wrote each other's tables directly, so nobody could change a schema with confidence.
- **Teams stepped on each other.** <mark class="todo">[A concrete example: a merge conflict, a release blocked by another team's change, a slow build.]</mark>

None of these is a reason to adopt microservices on its own. Together, they meant the architecture was now slowing the team down more than it was helping.

<figure class="diagram-figure">
    <div class="diagram-pair">
        <svg class="diagram" viewBox="0 0 340 260" role="img" aria-labelledby="d-before">
            <title id="d-before">Before: four modules in one deployable, all sharing one database</title>
            <rect class="d-frame" x="10" y="10" width="320" height="150" rx="12"/>
            <text class="d-label" x="170" y="32">Monolith · one deployable</text>
            <rect x="30" y="45" width="130" height="44" rx="8"/><text x="95" y="72">Module A</text>
            <rect x="180" y="45" width="130" height="44" rx="8"/><text x="245" y="72">Module B</text>
            <rect x="30" y="100" width="130" height="44" rx="8"/><text x="95" y="127">Module C</text>
            <rect x="180" y="100" width="130" height="44" rx="8"/><text x="245" y="127">Module D</text>
            <line class="d-line" x1="170" y1="160" x2="170" y2="192"/>
            <path class="d-db" d="M120 200 v40 a50 9 0 0 0 100 0 v-40"/>
            <ellipse class="d-db" cx="170" cy="200" rx="50" ry="9"/>
            <text x="170" y="232">Shared DB</text>
        </svg>
        <svg class="diagram" viewBox="0 0 340 290" role="img" aria-labelledby="d-after">
            <title id="d-after">After: an API gateway in front of three services, each with its own database, publishing events to a message bus</title>
            <rect x="95" y="10" width="150" height="36" rx="8"/><text x="170" y="33">API gateway</text>
            <line class="d-line" x1="140" y1="46" x2="60" y2="80"/>
            <line class="d-line" x1="170" y1="46" x2="170" y2="80"/>
            <line class="d-line" x1="200" y1="46" x2="280" y2="80"/>
            <rect x="10" y="80" width="100" height="44" rx="8"/><text x="60" y="107">Service A</text>
            <rect x="120" y="80" width="100" height="44" rx="8"/><text x="170" y="107">Service B</text>
            <rect x="230" y="80" width="100" height="44" rx="8"/><text x="280" y="107">Service C</text>
            <line class="d-line" x1="60" y1="124" x2="60" y2="146"/>
            <line class="d-line" x1="170" y1="124" x2="170" y2="146"/>
            <line class="d-line" x1="280" y1="124" x2="280" y2="146"/>
            <path class="d-db" d="M35 152 v28 a25 6 0 0 0 50 0 v-28"/><ellipse class="d-db" cx="60" cy="152" rx="25" ry="6"/>
            <path class="d-db" d="M145 152 v28 a25 6 0 0 0 50 0 v-28"/><ellipse class="d-db" cx="170" cy="152" rx="25" ry="6"/>
            <path class="d-db" d="M255 152 v28 a25 6 0 0 0 50 0 v-28"/><ellipse class="d-db" cx="280" cy="152" rx="25" ry="6"/>
            <line class="d-line d-dashed" x1="20" y1="124" x2="20" y2="238"/>
            <line class="d-line d-dashed" x1="130" y1="124" x2="130" y2="238"/>
            <line class="d-line d-dashed" x1="240" y1="124" x2="240" y2="238"/>
            <rect class="d-bus" x="10" y="238" width="320" height="40" rx="8"/><text x="170" y="263">Message bus · events</text>
        </svg>
    </div>
    <figcaption>Before and after, simplified. <mark class="todo">[Rename the modules and services to your real business capabilities, or keep them generic.]</mark></figcaption>
</figure>

## 1. Draw boundaries around the business, not the code

The tempting first cut is technical: an "API service", a "data service", a "notifications service". That gives you a distributed monolith, where every feature still touches every service.

What worked better was to start from **business capabilities** and ask three questions of each candidate service:

1. **Does it own its data?** If two services must write the same table, they're one service, or the boundary is in the wrong place.
2. **Can one team own it end to end?** A service with no clear owner becomes everyone's problem.
3. **Can it change and deploy on its own most of the time?** If a typical feature needs coordinated releases of three services, the lines are wrong.

We ended up with <mark class="todo">[N]</mark> services. The first one we extracted was <mark class="todo">[which one, and why it was a good first candidate: low risk, clear data, high change rate?]</mark>.

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
