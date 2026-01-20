# Пошаговое развертывание с проверкой в CI/CD

## Добавил эксперементально работу с векторной базой данных Qdrant для хранения векторных представлений.

### Что сделано по Qdrant (экспериментально)

На текущий момент в репозитории Qdrant рассматривается как **план/эксперимент для интеграции** (как векторное хранилище для embeddings), а основная микросервисная часть остаётся на MongoDB/Redis/RabbitMQ.

Сейчас в проекте:

- **Задекларирован сценарий использования Qdrant**: хранение embedding’ов + поиск ближайших векторов (vector similarity search) для задач вроде:
  - подбор релевантных страниц/материалов,
  - похожие вопросы/ответы,
  - поиск по смыслу.
- **Описан целевой контракт данных** (что планируется хранить в Qdrant):
  - `id` точки (например, `Guid` страницы/материала),
  - `vector` (массив float),
  - `payload` (метаданные: тип сущности, заголовок, теги, язык, дата обновления и т.п.).
- **Выбран способ доступа**: HTTP API Qdrant (позже можно перейти на официальный клиент).

Что ещё нужно, чтобы эксперимент реально работал end-to-end (следующие шаги)

1) Добавить сервис `qdrant` в `docker-compose.microservices.yml` (инфраструктура рядом с MongoDB/Redis/RabbitMQ):
- поднять контейнер `qdrant/qdrant`,
- пробросить порт `6333`,
- добавить volume для хранения данных.

2) Добавить конфигурацию в `appsettings.json` API Gateway (или отдельного сервиса):
- базовый URL `Qdrant__BaseUrl` (например, `http://qdrant:6333` в compose),
- имя коллекции `Qdrant__Collection`.

3) Реализовать минимальный слой интеграции (например в `SteelDesignerEngineer.Infrastructure`):
- create collection при старте (если не существует),
- upsert точки (id + vector + payload),
- поиск `search` по вектору.

4) Добавить API/hander для ручной проверки:
- endpoint в gateway типа `POST /api/vector/search`.

#### Как использовать поиск (строка поиска → вектор → Qdrant)

Qdrant **не ищет по строке напрямую**: он ищет по **векторам**. Поэтому пайплайн для строкового поиска обычно такой:

1) Пользователь вводит строку `query`.
2) Сервис получает embedding для `query` через модель (OpenAI/локальная модель/другая).
3) Embedding (массив float) отправляется в Qdrant на поиск ближайших векторов.
4) Из результатов берём `id`/`payload` и подтягиваем детали (страницу, материал) из MongoDB.

##### Пример запроса в Qdrant: поиск ближайших векторов

`POST /collections/<collection>/points/search`

Поля:
- `vector`: embedding запроса
- `limit`: сколько результатов вернуть
- `with_payload`: вернуть payload вместе с результатами
- `filter`: фильтрация по payload (опционально)

Пример (упрощённый):

```http
POST http://localhost:6333/collections/pagecontents/points/search
Content-Type: application/json

{
  "vector": [0.12, -0.98, 0.33, 0.01],
  "limit": 5,
  "with_payload": true
}
```

##### Пример фильтрации результатов

Например, ограничить поиск только страницами определённого типа/языка:

```http
POST http://localhost:6333/collections/pagecontents/points/search
Content-Type: application/json

{
  "vector": [0.12, -0.98, 0.33, 0.01],
  "limit": 5,
  "with_payload": true,
  "filter": {
    "must": [
      { "key": "type", "match": { "value": "page" } },
      { "key": "lang", "match": { "value": "ru" } }
    ]
  }
}
```

##### Рекомендации по параметрам поиска

- `limit`: 5–20 для UI сценариев; больше — для batch-обработки.
- `with_payload`: удобно для UI, но увеличивает ответ; можно возвращать только `id` и дальше получать данные из MongoDB.
- `score`: у Qdrant это similarity score — чем выше, тем ближе; стоит вводить порог (например `minScore` на стороне приложения).
- `filter`: используйте для ограничения домена поиска (роль, предмет, язык, доступность).

> Важно: для работы поиска нужна отдельная система получения embedding’ов. В этом репозитории её пока нет — это следующий шаг интеграции.

## Обзор

Руководство по развертыванию микросервисов Steel Designer Engineer в Kubernetes.

**Параметры (без конфиденциальных данных):**
- **Namespace:** `steel-design-engineer`
- **Ingress controller:** Traefik (namespace: `traefik`)
- **Docker Registry:** Docker Hub
- **Тег образов:** `v1.0.0`
- **Компоненты:**
  - `SteelDesignerEngineer.ApiGateway`
  - `SteelDesignerEngineer.Services.Auth` (worker)
  - `SteelDesignerEngineer.Services.Session` (worker)
  - `SteelDesignerEngineer.Services.PageContent` (worker)
- **K8s манифесты:** `SteelDesignerEngineer.ApiGateway/k8s/`
- **Время развертывания:** ~3–10 минут (зависит от pull образов)

---

## Предварительные требования

- Kubernetes кластер с доступом
- Docker + buildx доступны там, где собираешь образы
- Доступ к Docker Registry (Docker Hub / GHCR / Harbor)
- Traefik установлен (namespace: `traefik`) и будет использоваться для внешнего доступа
- DNS домен → внешний IP Traefik (или внешний IP ноды/балансера)

**Быстрая проверка:**

```sh
kubectl version --client
kubectl config current-context
kubectl get nodes
kubectl get ns steel-design-engineer

# traefik
kubectl get deploy -n traefik traefik
kubectl get svc -n traefik
```

---

## Структура файлов

```
SteelDesignerEngineer.ApiGateway/k8s/
+-- configs/
¦   L-- steel-configmap.yaml
+-- secrets/
¦   L-- steel-secrets.yaml
+-- deployments/
¦   +-- steel-api-gateway-deployment.yaml
¦   +-- steel-auth-service-deployment.yaml
¦   +-- steel-session-service-deployment.yaml
¦   L-- steel-pagecontent-service-deployment.yaml
L-- services/
    L-- steel-services.yaml

SteelDesignerEngineer.ApiGateway/k8s/old/
L-- steel_design_engineer_ingress.yaml  # Ingress (Traefik + TLS)

SteelDesignerEngineer.ApiGateway/k8s/old/ (рабочий эталон)
+-- steel_design_engineer_configmap.yaml
+-- steel_design_engineer_deployment.yaml
+-- steel_design_engineer_service.yaml
+-- steel_design_engineer_ingress.yaml
L-- steel_design_engineer_ingressroute.yaml (НЕ используется: требует Traefik CRD)
```

---

## Развертывание (пошагово)

### Шаг 0: Проверить доступ к кластеру

```sh
kubectl version --client
kubectl config current-context
kubectl get nodes
```

---

### Шаг 1: Проверить, что Traefik установлен

```sh
kubectl get deploy -n traefik traefik
kubectl get svc -n traefik
```

---

### Шаг 1.1: Проверить, что в Traefik настроен certResolver из Ingress

В `SteelDesignerEngineer.ApiGateway/k8s/old/steel_design_engineer_ingress.yaml` используется не стандартный certResolver: `mytlschallenge`.

Проверь, что он реально прописан в аргументах/конфиге Traefik:

```sh
kubectl -n traefik get deploy traefik -o yaml | grep -i "certificatesresolvers" -n || true
kubectl -n traefik get deploy traefik -o yaml | grep -i "mytlschallenge" -n || true
```

Если `mytlschallenge` не найден, TLS через аннотацию работать не будет.

---

### Шаг 1.2: Убедиться, что Traefik CRD НЕ используется

Проверка, что в кластере нет CRD `IngressRoute` (и ты их не применял):

```sh
kubectl get crd | grep -i traefik || true
kubectl get crd | grep -i ingressroute || true
```

Проверка, что в namespace НЕ создано IngressRoute (если CRD всё же установлен):

```sh
kubectl get ingressroute -n steel-design-engineer
```

Правильная картина для этого гайда: используется только `Ingress`:

```sh
kubectl get ingress -n steel-design-engineer
```

Дополнительно: убедиться, что в локальной папке с манифестами нет CRD-манифестов.

Команды ниже можно запускать из любой директории. Они найдут корень репозитория и проверят манифесты.

```sh
REPO_DIR="$(git rev-parse --show-toplevel 2>/dev/null || true)"

if [ -z "$REPO_DIR" ]; then
  REPO_DIR="$(find ~ -maxdepth 4 -type d -name "SteelDesignerEngineer.ApiGateway" 2>/dev/null | head -n 1 | sed 's#/SteelDesignerEngineer.ApiGateway##')"
fi

echo "Repo: $REPO_DIR"

grep -R "traefik.io/v1alpha1" -n "$REPO_DIR/SteelDesignerEngineer.ApiGateway/k8s/" 2>/dev/null || true
grep -R "kind: IngressRoute" -n "$REPO_DIR/SteelDesignerEngineer.ApiGateway/k8s/" 2>/dev/null || true
```

Если вывод пустой — CRD-манифесты не используются.

---

### Шаг 1.3: Проверить IngressClass

```sh
kubectl get ingressclass
```

---

### Шаг 1.4: Логи Traefik (полезно для TLS/certResolver)

```sh
kubectl -n traefik logs deploy/traefik --tail=200
```

---

### Шаг 2: Создать namespace

```sh
kubectl create namespace steel-design-engineer
```

---

### Шаг 3: Проверить namespace

```sh
kubectl get ns steel-design-engineer
```

---

### Шаг 4: Собрать Docker-образы (4 сервиса)

```sh
docker build -t nnshchegolev/steel-api-gateway:v1.0.0 -f SteelDesignerEngineer.ApiGateway/Dockerfile .
docker build -t nnshchegolev/steel-auth-service:v1.0.0 -f SteelDesignerEngineer.Services.Auth/Dockerfile .
docker build -t nnshchegolev/steel-session-service:v1.0.0 -f SteelDesignerEngineer.Services.Session/Dockerfile .
docker build -t nnshchegolev/steel-pagecontent-service:v1.0.0 -f SteelDesignerEngineer.Services.PageContent/Dockerfile .
```

---

### Шаг 5: Push Docker-образов

```sh
docker push nnshchegolev/steel-api-gateway:v1.0.0
docker push nnshchegolev/steel-auth-service:v1.0.0
docker push nnshchegolev/steel-session-service:v1.0.0
docker push nnshchegolev/steel-pagecontent-service:v1.0.0
```

---

### Шаг 6: Проверить доступность Docker-образов (pull)

```sh
docker pull nnshchegolev/steel-api-gateway:v1.0.0
docker pull nnshchegolev/steel-auth-service:v1.0.0
docker pull nnshchegolev/steel-session-service:v1.0.0
docker pull nnshchegolev/steel-pagecontent-service:v1.0.0
```

---

### Шаг 7: Посмотреть `image:` в Deployment манифестах

```sh
grep -R "^\s*image:" -n SteelDesignerEngineer.ApiGateway/k8s/deployments/
```

---

### Шаг 8: Применить ConfigMap

```sh
kubectl apply -n steel-design-engineer -f SteelDesignerEngineer.ApiGateway/k8s/configs/steel-configmap.yaml
```

---

### Шаг 9: Проверить ConfigMap

```sh
kubectl get configmap -n steel-design-engineer steel-designer-engineer-config
```

---

### Шаг 10: Применить Deployments

```sh
kubectl apply -n steel-design-engineer -f SteelDesignerEngineer.ApiGateway/k8s/deployments/
```

---

### Шаг 11: Проверить Pods

```sh
kubectl get pods -n steel-design-engineer -o wide
```

---

### Шаг 12: Дождаться готовности (rollout)

```sh
kubectl rollout status -n steel-design-engineer deploy/steel-api-gateway --timeout=300s
kubectl rollout status -n steel-design-engineer deploy/steel-auth-service --timeout=300s
kubectl rollout status -n steel-design-engineer deploy/steel-session-service --timeout=300s
kubectl rollout status -n steel-design-engineer deploy/steel-pagecontent-service --timeout=300s
```

---

### Шаг 13: Применить Services

```sh
kubectl apply -n steel-design-engineer -f SteelDesignerEngineer.ApiGateway/k8s/services/steel-services.yaml
```

---

### Шаг 14: Проверить Services

```sh
kubectl get svc -n steel-design-engineer
```

---

### Шаг 15: Проверить Endpoints

```sh
kubectl get endpoints -n steel-design-engineer
```

---

### Шаг 16: Применить Ingress

```sh
kubectl apply -f SteelDesignerEngineer.ApiGateway/k8s/old/steel_design_engineer_ingress.yaml
```

---

### Шаг 17: Проверить Ingress

```sh
kubectl get ingress -n steel-design-engineer
kubectl describe ingress -n steel-design-engineer steel-designer-engineer-ingress
```

---

### Шаг 18: Проверить TLS secret

```sh
kubectl get secret -n steel-design-engineer steel-designer-engineer-tls
```

---

### Шаг 19: Внешний тест (HTTP)

```sh
curl -I http://steel-designer-engineer.ru/
```

---

### Шаг 20: Внешний тест (HTTPS)

```sh
curl -I https://steel-designer-engineer.ru/
```

---

### Шаг 21: Внешний тест (полный ответ)

```sh
curl -i https://steel-designer-engineer.ru/
```

---

### Шаг 22: Диагностика (если что-то не стартует)

```sh
kubectl get events -n steel-design-engineer --sort-by=.lastTimestamp | tail -n 50
kubectl get pods -n steel-design-engineer -o wide
kubectl describe pod -n steel-design-engineer <pod-name>
kubectl logs -n steel-design-engineer <pod-name> --tail=200

```

---

## Важно (согласованность манифестов)

1) В `SteelDesignerEngineer.ApiGateway/k8s/old/steel_design_engineer_ingress.yaml` backend указывает на сервис `steel-designer-engineёр-service`, но в репозитории сервис называется `steel-api-gateway` (файл `SteelDesignerEngineer.ApiGateway/k8s/services/steel-services.yaml`).

Чтобы Ingress работал, в `steel_design_engineer_ingress.yaml` должно быть:

- `backend.service.name: steel-api-gateway`

2) В `steel-services.yaml` у `steel-api-gateway` стоит `targetPort: 5000`, но в deployment контейнер слушает `80` (`ASPNETCORE_URLS=http://+:80`).

Чтобы сервис работал, у `steel-api-gateway` должно быть:

- `targetPort: 80`

3) Файл `SteelDesignerEngineer.ApiGateway/k8s/secrets/steel-secrets.yaml` содержит реальные креды (их нельзя хранить в репозитории). Создавай секреты вручную через `kubectl create secret ...` или используй отдельный защищенный канал хранения.

4) Образы должны совпадать
+- В deployments указаны 4 разных `image:` (gateway/auth/session/pagecontent). Это значит, что надо пушить 4 образа с этими же именами и тегом.

5) `imagePullSecrets`
+- Если образы **public**, `imagePullSecrets` не требуется. Если образы **private**, нужен secret и его имя должно совподать с `imagePullSecrets.name`.

Если что-то не стартует — начинать с `kubectl describe pod ...` и смотреть `Events` (ImagePullBackOff / CrashLoopBackOff).

## Аудит связности (что должно совпадать, чтобы деплой реально завёлся)

Проверено по файлам в проекте:

1) `Ingress` > `Service`
- `SteelDesignerEngineer.ApiGateway/k8s/old/steel_design_engineer_ingress.yaml` должен проксировать на сервис `steel-api-gateway:80`.

2) `Service` > `Deployment` (порты)
- `SteelDesignerEngineer.ApiGateway/k8s/services/steel-services.yaml` для `steel-api-gateway` должен иметь `port: 80` и `targetPort: 80`.
- В `SteelDesignerEngineer.ApiGateway/k8s/deployments/steel-api-gateway-deployment.yaml` контейнер должен слушать `80`.
- `SteelDesignerEngineer.ApiGateway/k8s/services/steel-services.yaml` для `steel-auth-service/steel-session-service/steel-pagecontent-service` должен иметь `targetPort: 80`.
- В deployments этих сервисов контейнеры слушают `80`.

3) `ConfigMap` имя должно совпадать
- В deployments используется `configMap.name: steel-designer-engineер-config`.
- В `SteelDesignerEngineer.ApiGateway/k8s/configs/steel-configmap.yaml` имя тоже `steel-designer-engineер-config`.

4) Образы должны совпадать
- В deployments указаны 4 разных `image:` (gateway/auth/session/pagecontent). Это значит, что надо пушить 4 образа с этими же именами и тегом.

5) `imagePullSecrets`
- Если образы **public**, `imagePullSecrets` не требуется. Если образы **private**, нужен secret и его имя должно совподать с `imagePullSecrets.name`.

Если что-то не стартует — начинать с `kubectl describe pod ...` и смотреть `Events` (ImagePullBackOff / CrashLoopBackOff).

### Эталон для сравнения

В папке `SteelDesignerEngineer.ApiGateway/k8s/old/` лежит рабочая (ранее использованная) схема деплоя **монолита** `steel-designer-engineer` (один Deployment/Service/Ingress).

Текущая схема в `SteelDesignerEngineer.ApiGateway/k8s/` — это **микросервисы** (4 Deployment + 4 Service + Ingress только на gateway).

Проверка должна быть по тем же принципам, что и у `old`:

- `Deployment.labels` - `Service.selector`
- `Service.targetPort` - реальный порт контейнера
- `Ingress.backend.service.name` - существующий Service

CRD (IngressRoute) не используется.
