import express, { Express, Request } from 'express'
import morgan from 'morgan'

const app: Express = express()

app.use(morgan('dev'))
app.use(express.urlencoded({ extended: false }))
app.use(express.json())

app.post('/github', (req: Request, res: Request) => {
    console.log(req.body)
    // @ts-expect-error
    res.send('JD')
})

app.listen(3000, () => {
    console.log(`App Listening at http://localhost:3000`)
})